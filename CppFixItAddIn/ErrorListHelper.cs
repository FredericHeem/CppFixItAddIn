
namespace CppFixItAddIn
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.TextManager.Interop;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio;
    using System.Diagnostics;

    internal sealed class ErrorListHelper :
        IServiceProvider, IDisposable
    {
        private ErrorListProvider _errorProvider;
        private ServiceProvider _serviceProvider;
		private int _errorsSinceSuspend = 0;

        public int ErrorCount
        {
            get
            {
                return _errorProvider.Tasks.Count;
            }
        }

        public ErrorListHelper(object dte2)
        {
            _serviceProvider = new ServiceProvider(dte2 as Microsoft.VisualStudio.OLE.Interop.IServiceProvider);
            _errorProvider = new ErrorListProvider(_serviceProvider);//this implementing IServiceProvider
            _errorProvider.ProviderName = "CppFixIt";
            _errorProvider.ProviderGuid = new Guid(); // should be package guid
            _errorProvider.Show();
        }

        public void Show()
        {
            _errorProvider.Show();
            //_errorProvider.
           
        }

        public object GetService(Type serviceType)
        {
            return Package.GetGlobalService(serviceType);
        }

        public void SuspendRefresh()
        {
            _errorProvider.SuspendRefresh();
			_errorsSinceSuspend = 0;
        }

        public void ResumeRefresh()
        {
            _errorProvider.ResumeRefresh();

			if (_errorsSinceSuspend > 0)
			{
				_errorProvider.Show();
				_errorProvider.BringToFront();
			}
        }

        public void Write(
            TaskCategory category,
            TaskErrorCategory errorCategory,
            string text,
            string document,
            int line,
            int column)
        {
			_errorsSinceSuspend++;

            ErrorTask task = new ErrorTask();
            task.Text = text;
            task.ErrorCategory = errorCategory;
            //The task list does +1 before showing this numbers
            task.Line = line - 1;
            task.Column = column - 1;
            task.Document = document;
            task.Category = category;

            if (!string.IsNullOrEmpty(document))
            {
                //attach to the navigate event
                task.Navigate += NavigateDocument;
            }
            _errorProvider.Tasks.Add(task);
        }

        public void Clear()
        {
            _errorProvider.Tasks.Clear();
        }

        public void ClearDocument(string document)
        {
            for (int i = _errorProvider.Tasks.Count - 1; i >= 0; i--)
            {
                if ((string.IsNullOrEmpty(document) && string.IsNullOrEmpty(_errorProvider.Tasks[i].Document)) || _errorProvider.Tasks[i].Document == document)
                {
                    _errorProvider.Tasks.RemoveAt(i);
                }
            }
        }

        void NavigateDocument(object sender, EventArgs e)
        {
            Task task = sender as Task;
            if (task == null)
            {
                throw new ArgumentException("sender");
            }

            OpenDocumentAndNavigateTo(task.Document, task.Line, task.Column);
        }

        public void OpenDocumentAndNavigateTo(
                                    string path, int line, int column)
        {
            IVsUIShellOpenDocument openDoc = _serviceProvider.GetService(typeof(IVsUIShellOpenDocument)) as IVsUIShellOpenDocument;

            if (openDoc == null)
            {
                return;
            }
            IVsWindowFrame frame;
            Microsoft.VisualStudio.OLE.Interop.IServiceProvider sp;
            IVsUIHierarchy hier;
            uint itemid;
            Guid logicalView = VSConstants.LOGVIEWID_Code;
            if (ErrorHandler.Failed(
                openDoc.OpenDocumentViaProject(path, ref logicalView, out sp, out hier, out itemid, out frame))
                || frame == null)
            {
                return;
            }
            object docData;
            frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocData, out docData);

            // Get the VsTextBuffer  
            VsTextBuffer buffer = docData as VsTextBuffer;
            if (buffer == null)
            {
                IVsTextBufferProvider bufferProvider = docData as IVsTextBufferProvider;
                if (bufferProvider != null)
                {
                    IVsTextLines lines;
                    ErrorHandler.ThrowOnFailure(bufferProvider.GetTextBuffer(out lines));
                    buffer = lines as VsTextBuffer;
                    Debug.Assert(buffer != null, "IVsTextLines does not implement IVsTextBuffer");
                    if (buffer == null)
                    {
                        return;
                    }
                }
            }
            // Finally, perform the navigation.  
            IVsTextManager mgr = _serviceProvider.GetService(typeof(VsTextManagerClass))
                 as IVsTextManager;
            if (mgr == null)
            {
                return;
            }
            mgr.NavigateToLineAndColumn(buffer, ref logicalView, line, column, line, column);
        }

        #region IDisposable Members

        public void Dispose()
        {
            _errorProvider.Dispose();
            _serviceProvider.Dispose();
        }

        #endregion
    }

}
