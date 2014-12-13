using System;
using Extensibility;
using EnvDTE;
using EnvDTE80;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.VisualStudio.CommandBars;
using Microsoft.VisualStudio.VCProject;
using Microsoft.VisualStudio.VCProjectEngine;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Shell.Interop;

namespace CppFixItAddIn
{
	/// <summary>The object for implementing an Add-in.</summary>
	/// <seealso class='IDTExtensibility2' />
	public class Connect : IDTExtensibility2, IDTCommandTarget
	{
        private static readonly string _company = "CppFixIt";
        
        #region AddIn
        private DTE2 _applicationObject;
        private AddIn _addInInstance;
        private EnvDTE.Window _windowErrorView;
        #endregion 

        #region ctor
        /// <summary>Implements the constructor for the Add-in object. Place your initialization code within this method.</summary>
		public Connect(string  logPrefixName)
		{
            //AddInName = addInName;
            Tracer.Instance.LogFilePrefix = logPrefixName;
            ts = Tracer.Instance.CreateTraceSource("Connect");
            ts.TraceInformation("Connect ctor " + logPrefixName);
		}
        #endregion

        #region IDTExtensibility2
        /// <summary>Implements the OnConnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being loaded.</summary>
		/// <param term='application'>Root object of the host application.</param>
		/// <param term='connectMode'>Describes how the Add-in is being loaded.</param>
		/// <param term='addInInst'>Object representing this Add-in.</param>
		/// <seealso class='IDTExtensibility2' />
        public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
        {
            ts.TraceInformation("OnConnection " + connectMode.ToString());

            try
            {
                _applicationObject = (DTE2)application;
                _addInInstance = (AddIn)addInInst;
                

                switch (connectMode)
                {
                    case ext_ConnectMode.ext_cm_UISetup:
                        // Do nothing for this add-in with temporary user interface
                        break;

                    case ext_ConnectMode.ext_cm_Startup:
                        // The add-in was marked to load on startup
                        // Do nothing at this point because the IDE may not be fully initialized
                        // Visual Studio will call OnStartupComplete when fully initialized
                        break;

                    case ext_ConnectMode.ext_cm_AfterStartup:
                        // The add-in was loaded by hand after startup using the Add-In Manager
                        // Initialize it in the same way that when is loaded on startup
                        AddTemporaryUI();
                        break;
                }
            }
            catch (System.Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.ToString());
            }
        }

        /// <summary>Implements the OnDisconnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being unloaded.</summary>
		/// <param term='disconnectMode'>Describes how the Add-in is being unloaded.</param>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
		{
            ts.TraceInformation("OnDisconnection " + disconnectMode.ToString());
		}

		/// <summary>Implements the OnAddInsUpdate method of the IDTExtensibility2 interface. Receives notification when the collection of Add-ins has changed.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />		
		public void OnAddInsUpdate(ref Array custom)
		{
            ts.TraceInformation("OnAddInsUpdate");
		}

		/// <summary>Implements the OnStartupComplete method of the IDTExtensibility2 interface. Receives notification that the host application has completed loading.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnStartupComplete(ref Array custom)
		{
            ts.TraceInformation("OnStartupComplete");
            AddTemporaryUI();
            ts.TraceInformation("OnStartupComplete Done");
		}

		/// <summary>Implements the OnBeginShutdown method of the IDTExtensibility2 interface. Receives notification that the host application is being unloaded.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnBeginShutdown(ref Array custom)
		{
            ts.TraceInformation("OnBeginShutdown");
		}
        #endregion

        #region IDTCommandTarget

        private static string _addInName = "CppFixItAddIn";
        internal string AddInName
        {
            get { return Connect._addInName; }
            set { Connect._addInName = value; }
        }

        private static string _connectName = "Connect";
        internal static string ConnectName
        {
            get { return Connect._connectName; }
            set { Connect._connectName = value; }
        }
        
        private string _commandPrefix;
        internal string CommandPrefix
        {
            get { _commandPrefix = AddInName + "." + ConnectName  + "."; return _commandPrefix; }
        }

        
        /// <summary>Implements the QueryStatus method of the IDTCommandTarget interface. This is called when the command's availability is updated</summary>
        /// <param term='commandName'>The name of the command to determine state for.</param>
        /// <param term='neededText'>Text that is needed for the command.</param>
        /// <param term='status'>The state of the command in the user interface.</param>
        /// <param term='commandText'>Text requested by the neededText parameter.</param>
        /// <seealso class='Exec' />
        public void QueryStatus(string commandName, vsCommandStatusTextWanted neededText, ref vsCommandStatus status,
                        ref object commandText)
        {
            ts.TraceData(TraceEventType.Verbose, 1, "QueryStatus " + commandName);
            if (neededText == vsCommandStatusTextWanted.vsCommandStatusTextWantedNone)
            {
                if (commandName.StartsWith(CommandPrefix))
                {
                    status = vsCommandStatus.vsCommandStatusSupported;

                    if (commandName.EndsWith(COMMAND_ANALYSE_CURRENT_FILE_NAME))
                    {
                        ts.TraceData(TraceEventType.Verbose, 1, "QueryStatus " + COMMAND_ANALYSE_CURRENT_FILE_NAME + " " + _analyzing);
                        if (ClangAnalyzer.State == ClangAnalyzer.AnalyzeState.RUNNING)
                        {
                            status |= vsCommandStatus.vsCommandStatusInvisible;
                        }
                        else if (DTE2Utils.HasVcCppFileActive(_applicationObject.ActiveDocument))
                        {
                            status |= vsCommandStatus.vsCommandStatusEnabled;
                        }
                    }
                    if (commandName.EndsWith(COMMAND_ANALYSE_PROJECT_NAME))
                    {
                        ts.TraceData(TraceEventType.Verbose, 1, "QueryStatus " + COMMAND_ANALYSE_PROJECT_NAME + " " + _analyzing);
                        if (ClangAnalyzer.State == ClangAnalyzer.AnalyzeState.RUNNING)
                        {
                            status |= vsCommandStatus.vsCommandStatusInvisible;
                        }
                        else if (DTE2Utils.IsVcProjectSelected(_applicationObject))
                        {
                            status |= vsCommandStatus.vsCommandStatusEnabled;
                        }
                    }
                    if (commandName.EndsWith(COMMAND_ANALYSE_PROJECT_STARTUP_NAME))
                    {
                        ts.TraceData(TraceEventType.Verbose, 1, "QueryStatus " + COMMAND_ANALYSE_PROJECT_STARTUP_NAME + " " + _analyzing);
                        if (ClangAnalyzer.State == ClangAnalyzer.AnalyzeState.RUNNING)
                        {
                            status |= vsCommandStatus.vsCommandStatusInvisible;
                        }
                        else if (DTE2Utils.HasVcStartupProject(_applicationObject.Solution))
                        {
                            status |= vsCommandStatus.vsCommandStatusEnabled;
                        }
                    }
                    if (commandName.EndsWith(COMMAND_ANALYSE_SOLUTION_NAME))
                    {
                        ts.TraceData(TraceEventType.Verbose, 1, "QueryStatus " + COMMAND_ANALYSE_SOLUTION_NAME + " " + _analyzing);
                        if (ClangAnalyzer.State == ClangAnalyzer.AnalyzeState.RUNNING)
                        {
                            status |= vsCommandStatus.vsCommandStatusInvisible;
                        }
                        else if (DTE2Utils.HasVcProjectsInSolution(_applicationObject.Solution))
                        {
                            status |= vsCommandStatus.vsCommandStatusEnabled;
                        }
                    }

                    if (commandName.EndsWith(COMMAND_STOP_NAME))
                    {
                        ts.TraceData(TraceEventType.Verbose, 1, "QueryStatus " + COMMAND_STOP_NAME);
                        if (ClangAnalyzer.State == ClangAnalyzer.AnalyzeState.RUNNING)
                        {
                            status |= vsCommandStatus.vsCommandStatusEnabled;
                        }
                        else
                        {
                            status |= vsCommandStatus.vsCommandStatusInvisible;
                        }
                    }

                    if (commandName.EndsWith(COMMAND_SETTINGS_NAME))
                    {
                        ts.TraceData(TraceEventType.Verbose, 1, "QueryStatus " + COMMAND_SETTINGS_NAME + " ");
                        status |= vsCommandStatus.vsCommandStatusEnabled;
                    }
                    
                    if (commandName.EndsWith(COMMAND_OPENLOG_NAME))
                    {
                        ts.TraceData(TraceEventType.Verbose, 1, "QueryStatus " + COMMAND_OPENLOG_NAME + " On");
                        status |= vsCommandStatus.vsCommandStatusEnabled;
                    }

                    if (commandName.EndsWith(COMMAND_ABOUT_NAME))
                    {
                        ts.TraceData(TraceEventType.Verbose, 1, "QueryStatus " + COMMAND_ABOUT_NAME + " On");
                        status |= vsCommandStatus.vsCommandStatusEnabled;
                    }
                }
            }
        }

        /// <summary>Implements the Exec method of the IDTCommandTarget interface. This is called when the command is invoked.</summary>
        /// <param term='commandName'>The name of the command to execute.</param>
        /// <param term='executeOption'>Describes how the command should be run.</param>
        /// <param term='varIn'>Parameters passed from the caller to the command handler.</param>
        /// <param term='varOut'>Parameters passed from the command handler to the caller.</param>
        /// <param term='handled'>Informs the caller if the command was handled or not.</param>
        /// <seealso class='Exec' />
        public void Exec(string commandName, vsCommandExecOption executeOption, ref object varIn, ref object varOut,
                         ref bool handled)
        {
            ts.TraceInformation("Exec " + commandName);
            handled = false;
            if (executeOption == vsCommandExecOption.vsCommandExecOptionDoDefault)
            {
                String cmd = GetCommandShortName(commandName);
                handled = true;
                switch (cmd)
                {
                    case COMMAND_ANALYSE_CURRENT_FILE_NAME:
                        AnalyzeCurrentFile();
                        break;
                    case COMMAND_ANALYSE_PROJECT_NAME:
                        AnalyzeProject();
                        break;
                    case COMMAND_ANALYSE_PROJECT_STARTUP_NAME:
                        AnalyzeProjectStartup();
                        break;
                    case COMMAND_ANALYSE_SOLUTION_NAME:
                        AnalyzeSolution();
                        break;
                    case COMMAND_STOP_NAME:
                        StopAnalyze();
                        break;
                    case COMMAND_SETTINGS_NAME:
                        ShowSettings();
                        break;
                    case COMMAND_OPENLOG_NAME:
                        OpenLog();
                        break;
                    case COMMAND_ABOUT_NAME:
                        ShowAbout();
                        break;
                    default:
                        handled = false;
                        ts.TraceData(TraceEventType.Error, 1, "Exec unknow command " + commandName);
                        break;
                }
            }
        }

        private void StopAnalyze()
        {
            ts.TraceInformation("StopAnalyze");
            ClangAnalyzer.Stop();
        }

        private void ShowSettings()
        {
            using (SettingsDialog sd = new SettingsDialog())
            {
                if (sd.ShowDialog(null) == DialogResult.OK)
                {
                    
                }
            }
        }

        private void ShowAbout()
        {
            ts.TraceInformation("ShowAbout");
            try
            {
                using (AboutDialog aboutDialog = new AboutDialog())
                {
                    if (aboutDialog.ShowDialog(null) == DialogResult.OK)
                    {

                    }
                }
            }
            catch (Exception exception)
            {
                ts.TraceData(TraceEventType.Error, 1, "ShowAbout exception: " + exception.Message);
                ts.TraceData(TraceEventType.Error, 1, "ShowAbout exception: " + exception.StackTrace);
            }
            ts.TraceInformation("ShowAbout done");
        }

        private string GetCommandFullName(string commandName)
        {
            return CommandPrefix + commandName;
        }

        private string GetCommandShortName(string fullCmdName)
        {
            return fullCmdName.Remove(0, (CommandPrefix).Length);
        }

        #endregion

        #region Analser

        private bool _analyzing = false;
        private ClangAnalyzer _clangAnalyser;
        private ClangAnalyzer ClangAnalyzer
        {
            get
            {
                if (_clangAnalyser == null)
                {
                    _clangAnalyser = new ClangAnalyzer(_applicationObject);
                    _clangAnalyser.DelegateStartAnalyze += AnalyzerStartAnalyze;
                    _clangAnalyser.DelegateStopAnalyze += AnalyzerStopAnalyze;
                    _clangAnalyser.DelegateStartFile += AnalyzerStartFile;
                    _clangAnalyser.DelegateStopFile += AnalyzerStopFile;
                    _clangAnalyser.DelegateStartProject += AnalyzerStartProject;
                    _clangAnalyser.DelegateStopProject += AnalyzerStopProject;
                    _clangAnalyser.DelegateStartSolution += AnalyzerStartSolution;
                    _clangAnalyser.DelegateStopSolution += AnalyzerStopSolution;
                    _clangAnalyser.DelegateViolation += OnAnalyzerViolation;
                }

                return _clangAnalyser;
            }
        }

        private void OnAnalyzerViolation(Reporting.Violation violation)
        {
            ts.TraceInformation("OnAnalyzerViolation " + violation.FilePath +
                "(" + violation.LineNumber + "," + violation.ColumnNumber + "): " + violation.Message);

            TaskErrorCategory taskErrorCategory = Microsoft.VisualStudio.Shell.TaskErrorCategory.Error;
            if (violation.Severity == Reporting.Violation.ESeverity.WARNING)
            {
                taskErrorCategory = Microsoft.VisualStudio.Shell.TaskErrorCategory.Warning;
            }

            ErrorListHelper.Write(
                Microsoft.VisualStudio.Shell.TaskCategory.BuildCompile,
                taskErrorCategory,
                  violation.Message, 
                  violation.FilePath, 
                  Convert.ToInt32(violation.LineNumber),
                  Convert.ToInt32(violation.ColumnNumber));

        }

        private ErrorListHelper _errorListHelper;

        private ErrorListHelper ErrorListHelper
        {
            get
            {
                if (_errorListHelper == null)
                {
                    _errorListHelper = new ErrorListHelper(_applicationObject);
                }
                return _errorListHelper;
            }
        }

        /// <summary>
        /// Call from ClangAnalyzer.DelegateStartAnalyze
        /// </summary>
        /// <param name="success"></param>
        private void AnalyzerStartAnalyze(bool success)
        {
            ts.TraceInformation("AnalyzerDelegateStartFile");
            _analyzing = true;
            ErrorListHelper.Clear();
        }

        /// <summary>
        /// Call from ClangAnalyzer.DelegateStopAnalyze
        /// </summary>
        /// <param name="success"></param>
        private void AnalyzerStopAnalyze(bool success)
        {
            ts.TraceInformation("AnalyzerDelegateStopAnalyze");
            _analyzing = false;
            ErrorListHelper.Show();

        }

        /// <summary>
        /// Call from ClangAnalyzer.DelegateStartFile
        /// </summary>
        /// <param name="success"></param>
        private void AnalyzerStartFile(bool success)
        {
            ts.TraceInformation("AnalyzerDelegateStartFile");
        }

        /// <summary>
        /// Call from ClangAnalyzer.DelegateStopFile
        /// </summary>
        /// <param name="success"></param>
        private void AnalyzerStopFile(bool success)
        {
            ts.TraceInformation("AnalyzerDelegateStopFile");
        }

        /// <summary>
        /// Call from ClangAnalyzer.DelegateStartProject
        /// </summary>
        /// <param name="success"></param>
        private void AnalyzerStartProject(bool success)
        {
            ts.TraceInformation("AnalyzerDelegateStartProject");
        }

        /// <summary>
        /// Call from ClangAnalyzer.DelegateStopProject
        /// </summary>
        /// <param name="success"></param>
        private void AnalyzerStopProject(bool success)
        {
            ts.TraceInformation("AnalyzerDelegateStopProject");
        }

        /// <summary>
        /// Call from ClangAnalyzer.DelegateStartSolution
        /// </summary>
        /// <param name="success"></param>
        private void AnalyzerStartSolution(bool success)
        {
            ts.TraceInformation("AnalyzerDelegateStartSolution");
            
        }

        /// <summary>
        /// Call from ClangAnalyzer.DelegateStopSolution
        /// </summary>
        /// <param name="success"></param>
        private void AnalyzerStopSolution(bool success)
        {
            ts.TraceInformation("AnalyzerDelegateStopSolution");
        }

        private void AnalyzeCurrentFile()
        {
            ts.TraceInformation("AnalyzeCurrentFile");

            try
            {
                VCFile vcFile = DTE2Utils.GetVcCppFile(_applicationObject.ActiveDocument);
                if (vcFile == null)
                {
                    return;
                }

                VCConfiguration vcConfiguration = DTE2Utils.GetVcConfigurationForDocument(_applicationObject.ActiveDocument, vcFile.project);
                if (vcConfiguration == null)
                {
                    return;
                }

                ClangAnalyzer.AnalyzeFile(_applicationObject.Solution, vcFile, vcConfiguration);
            }
            catch (Exception exception)
            {
                ts.TraceData(TraceEventType.Error, 1, "AnalyzeFile exception: " + exception.Message);
                MessageBox.Show(exception.Message, _addInName + " System Error", MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            }
        }

        private void AnalyzeProject()
        {
            ts.TraceInformation("AnalyzeProject");

            try
            {
                Solution solution = _applicationObject.Solution;
               Array projects = (Array)_applicationObject.ActiveSolutionProjects;
               if (projects.Length > 0)
               {
                   Project project = (Project)projects.GetValue(0);
                   VCProject vcProject = project.Object as VCProject;
                   if (vcProject != null)
                   {
                       ClangAnalyzer.AnalyzeProject(solution, vcProject);
                   }
                   else 
                   {
                       ts.TraceInformation("AnalyzeProject not a vc project: " + project.Name);
                   }
               }
            }

            catch (Exception exception) 
            {
                ts.TraceData(TraceEventType.Error, 1, "AnalyzeProject exception: " + exception.Message);
                MessageBox.Show(exception.Message, _addInName + " System Error", MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }

        private void AnalyzeProjectStartup()
        {
            ts.TraceInformation("AnalyzeProjectStartup");

            try
            {
                Solution solution = _applicationObject.Solution;
                VCProject vcProject = DTE2Utils.GetVcProjectStartup(solution);
                if (vcProject == null)
                {
                    ts.TraceInformation("AnalyzeProjectStartup cannot get startup vc project from solution");
                    return;
                }

                ClangAnalyzer.AnalyzeProject(solution, vcProject);
            }
            catch (Exception exception)
            {
                ts.TraceData(TraceEventType.Error, 1, "AnalyzeProjectStartup exception: " + exception.Message);
                MessageBox.Show(exception.Message, _addInName + " System Error", MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }

        private void AnalyzeSolution()
        {
            ts.TraceInformation("AnalyzeSolution");

            try
            {
                Solution solution = _applicationObject.Solution;
                ClangAnalyzer.AnalyzeSolution(solution);
            }
            catch (Exception exception)
            {
                ts.TraceData(TraceEventType.Error, 1, "AnalyzeSolution exception: " + exception.Message);
                MessageBox.Show(exception.Message, _addInName + " System Error", MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Log
        private TraceSource ts;
        private void OpenLog()
        {
            ts.TraceInformation("OpenLog");
            try
            {
                System.Diagnostics.Process.Start(Tracer.Instance.GetLogFileAbsolute());
            }
            catch (System.Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.ToString());
            }
        }

        #endregion

        #region Ui

        private const string VS_MENUBAR_COMMANDBAR_NAME = "MenuBar";
        private const string VS_SOLUTIONEXPLORERFILE_COMMANDBAR_NAME = "Item";
        private const string VS_SOLUTIONEXPLORERPROJECT_COMMANDBAR_NAME = "Project";
        private const string VS_SOLUTIONEXPLORERSOLUTION_COMMANDBAR_NAME = "Solution";
        private const string VS_CODE_COMMANDBAR_NAME = "Code Window";

        private const string COMMANDBAR_ADDIN_NAME = "CppFixIt";
        private const string COMMANDBAR_ADDIN_CAPTION = "CppFixIt";
        private const string COMMANDBAR_ADDIN_TOOLTIP = "CppFixIt";

        private CommandBarButton _commandBarButtonAnalyzeCurrentFile;
        private CommandBarButton _commandBarButtonCodeAnalyzeCurrentFile;
        private CommandBarButton _commandBarButtonSolutionExplorerAnalyzeCurrentFile;

        private const string COMMAND_ANALYSE_CURRENT_FILE_NAME = "AnalyzeCurrentFile";
        private const string COMMAND_ANALYSE_CURRENT_FILE_CAPTION = "Analyze File";
        private const string COMMAND_ANALYSE_CURRENT_FILE_TOOLTIP = "Analyze File";

        private CommandBarButton _commandBarButtonMenuBarAnalyzeProjectStartup;
        //private CommandBarButton _commandBarButtonCodeAnalyzeProject;
        private CommandBarButton _commandBarButtonSolutionExplorerAnalyzeProject;

        private const string COMMAND_ANALYSE_PROJECT_STARTUP_NAME = "AnalyzeProjectStartup";
        private const string COMMAND_ANALYSE_PROJECT_NAME = "AnalyzeProject";
        private const string COMMAND_ANALYSE_PROJECT_CAPTION = "Analyze Project";
        private const string COMMAND_ANALYSE_PROJECT_TOOLTIP = "Analyze Project";

        private CommandBarButton _commandBarButtonAnalyzeSolution;
        private CommandBarButton _commandBarButtonCodeAnalyzeSolution;
        private CommandBarButton _commandBarButtonSolutionExplorerAnalyzeSolution;
        private const string COMMAND_ANALYSE_SOLUTION_NAME = "AnalyzeSolution";
        private const string COMMAND_ANALYSE_SOLUTION_CAPTION = "Analyze Solution";
        private const string COMMAND_ANALYSE_SOLUTION_TOOLTIP = "Analyze Solution";

        private CommandBarButton _commandBarButtonMenuAbout;
        private CommandBarButton _commandBarButtonCodeAbout;
        private CommandBarButton _commandBarButtonSolutionExplorerAbout;
        private const string COMMAND_ABOUT_NAME = "About";
        private const string COMMAND_ABOUT_CAPTION = "About";
        private const string COMMAND_ABOUT_TOOLTIP = "About";

        private CommandBarButton _commandBarButtonMenuStop;
        private CommandBarButton _commandBarButtonCodeStop;
        private CommandBarButton _commandBarButtonSolutionExplorerStop;
        private const string COMMAND_STOP_NAME = "Stop";
        private const string COMMAND_STOP_CAPTION = "Stop analyzing";
        private const string COMMAND_STOP_TOOLTIP = "Stop analyzing";

        private CommandBarButton _commandBarButtonMenuSettings;
        private CommandBarButton _commandBarButtonCodeSettings;
        private CommandBarButton _commandBarButtonSolutionExplorerSettings;
        private const string COMMAND_SETTINGS_NAME = "Settings";
        private const string COMMAND_SETTINGS_CAPTION = "Settings ...";
        private const string COMMAND_SETTINGS_TOOLTIP = "Open settings dialog";

        private CommandBarButton _commandBarButtonMenuOpenLog;
        private CommandBarButton _commandBarButtonCodeOpenLog;
        private CommandBarButton _commandBarButtonSolutionExplorerOpenLog;
        private const string COMMAND_OPENLOG_NAME = "OpenLog";
        private const string COMMAND_OPENLOG_CAPTION = "Open Log";
        private const string COMMAND_OPENLOG_TOOLTIP = "Open the log file";

        private string GetCommandBarName()
        {
            string commandBarName = COMMANDBAR_ADDIN_NAME;
            if (ConnectName.EndsWith("Debug"))
            {
                commandBarName = commandBarName + " [DBG]";
            }

            return commandBarName;
        }

        private CommandBarPopup _commandBarPopupAddIn;
        private CommandBarPopup CommandBarPopupAddIn
        {
            get
            {
                if (_commandBarPopupAddIn == null)
                {

                    _commandBarPopupAddIn = GetCommandBarPopup(_addInInstance, VS_MENUBAR_COMMANDBAR_NAME, GetCommandBarName());
                }

                return _commandBarPopupAddIn;
            }
        }

        private CommandBarPopup _codePopupAddIn;
        private CommandBarPopup CodePopupAddIn
        {
            get
            {
                if (_codePopupAddIn == null)
                {
                    _codePopupAddIn = GetCommandBarPopup(_addInInstance, VS_CODE_COMMANDBAR_NAME, GetCommandBarName());
                }

                return _codePopupAddIn;
            }
        }

        private CommandBarPopup _solutionExplorerPopupAddIn;
        private CommandBarPopup SolutionExplorerFilePopup
        {
            get
            {
                if (_solutionExplorerPopupAddIn == null)
                {
                    _solutionExplorerPopupAddIn = GetCommandBarPopup(_addInInstance, VS_SOLUTIONEXPLORERFILE_COMMANDBAR_NAME, GetCommandBarName());
                }

                return _solutionExplorerPopupAddIn;
            }
        }

        private CommandBarPopup _solutionExplorerProjectPopup;
        private CommandBarPopup SolutionExplorerProjectPopup
        {
            get
            {
                if (_solutionExplorerProjectPopup == null)
                {
                    _solutionExplorerProjectPopup = GetCommandBarPopup(_addInInstance, VS_SOLUTIONEXPLORERPROJECT_COMMANDBAR_NAME, GetCommandBarName());
                }

                return _solutionExplorerProjectPopup;
            }
        }

        private CommandBarPopup _solutionExplorerSolutionPopupAddIn;
        private CommandBarPopup SolutionExplorerSolutionPopup
        {
            get
            {
                if (_solutionExplorerSolutionPopupAddIn == null)
                {
                    _solutionExplorerSolutionPopupAddIn = GetCommandBarPopup(_addInInstance, VS_SOLUTIONEXPLORERSOLUTION_COMMANDBAR_NAME, GetCommandBarName());
                }

                return _solutionExplorerSolutionPopupAddIn;
            }
        }

        private CommandBarPopup GetCommandBarPopup(AddIn addIn, string commandBarName, string name)
        {
            CommandBarPopup commandBarPopup = null;
            var commandBars = ((CommandBars)_applicationObject.CommandBars);

            CommandBar commandBar = commandBars[commandBarName];

            commandBarPopup = commandBar.Controls.Add(
                MsoControlType.msoControlPopup,
                Type.Missing,
                Type.Missing,
                commandBar.Controls.Count + 1,
                true) as CommandBarPopup;

            //TODO 
            commandBarPopup.Caption = name;
            commandBarPopup.CommandBar.Name = name;
            return commandBarPopup;
        }

        private Command GetCommand(AddIn addIn, string name, string caption, string tooltip)
        {
            Command command = null;
            object[] contextUIGuids = new object[] { };

            try
            {
                // Try to retrieve the command, just in case it was already created, ignoring the 
                // exception that would happen if the command was not created yet.
                try
                {
                    command = _applicationObject.Commands.Item(addIn.ProgID + "." + name, -1);
                }
                catch
                {
                }

                // Add the command if it does not exist
                if (command == null)
                {
                    command = _applicationObject.Commands.AddNamedCommand(addIn, name, caption, tooltip, true);
                }

            }
            catch (System.Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.ToString());
            }

            return command;
        }

        private CommandBarButton CreateCommandBarButton(AddIn addIn, CommandBar commandBar, string name, string caption, string tooltip)
        {
            CommandBarButton commandBarButton = null;
            
            try
            {
                Command command = GetCommand(addIn, name, caption, tooltip);

                commandBarButton = command.AddControl(
                    commandBar, commandBar.Controls.Count + 1) as CommandBarButton;

            }
            catch (System.Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.ToString());
            }

            return commandBarButton;
        }

        private void AddTemporaryUI()
        {
            ts.TraceInformation("AddTemporaryUI");
      
            setupAnalyzeFile();
            setupAnalyzeProject();
            setupAnalyzeSolution();
            setupStop();
            
            

            setupSettings();
            setupOpenLog();
            setupAbout();
            //ShowErrorView();
        }

        private void setupAnalyzeFile()
        {
            //Analyze file
            _commandBarButtonAnalyzeCurrentFile = CreateCommandBarButton(
                _addInInstance, CommandBarPopupAddIn.CommandBar,
                COMMAND_ANALYSE_CURRENT_FILE_NAME, COMMAND_ANALYSE_CURRENT_FILE_CAPTION, COMMAND_ANALYSE_CURRENT_FILE_CAPTION);

            _commandBarButtonCodeAnalyzeCurrentFile = CreateCommandBarButton(
                _addInInstance, CodePopupAddIn.CommandBar,
                COMMAND_ANALYSE_CURRENT_FILE_NAME, COMMAND_ANALYSE_CURRENT_FILE_CAPTION, COMMAND_ANALYSE_CURRENT_FILE_CAPTION);

            _commandBarButtonSolutionExplorerAnalyzeCurrentFile = CreateCommandBarButton(
                            _addInInstance, SolutionExplorerFilePopup.CommandBar,
                            COMMAND_ANALYSE_CURRENT_FILE_NAME, COMMAND_ANALYSE_CURRENT_FILE_CAPTION, COMMAND_ANALYSE_CURRENT_FILE_CAPTION);

        }

        private void setupAnalyzeProject()
        {
            //Analyze project
            _commandBarButtonMenuBarAnalyzeProjectStartup = CreateCommandBarButton(
                _addInInstance, CommandBarPopupAddIn.CommandBar,
                COMMAND_ANALYSE_PROJECT_STARTUP_NAME, COMMAND_ANALYSE_PROJECT_CAPTION, COMMAND_ANALYSE_PROJECT_TOOLTIP);

            //_commandBarButtonCodeAnalyzeProject = CreateCommandBarButton(
            //    _addInInstance, CodePopupAddIn.CommandBar,
            //    COMMAND_ANALYSE_PROJECT_NAME, COMMAND_ANALYSE_PROJECT_CAPTION, COMMAND_ANALYSE_PROJECT_TOOLTIP);

            _commandBarButtonSolutionExplorerAnalyzeProject = CreateCommandBarButton(
                _addInInstance, SolutionExplorerProjectPopup.CommandBar,
                COMMAND_ANALYSE_PROJECT_NAME, COMMAND_ANALYSE_PROJECT_CAPTION, COMMAND_ANALYSE_PROJECT_TOOLTIP);

        }

        private void setupAnalyzeSolution()
        {
            //AnalyzeSolution
            _commandBarButtonAnalyzeSolution = CreateCommandBarButton(
                _addInInstance, CommandBarPopupAddIn.CommandBar,
                COMMAND_ANALYSE_SOLUTION_NAME, COMMAND_ANALYSE_SOLUTION_CAPTION, COMMAND_ANALYSE_SOLUTION_TOOLTIP);

            _commandBarButtonCodeAnalyzeSolution = CreateCommandBarButton(
                _addInInstance, CodePopupAddIn.CommandBar,
                COMMAND_ANALYSE_SOLUTION_NAME, COMMAND_ANALYSE_SOLUTION_CAPTION, COMMAND_ANALYSE_SOLUTION_TOOLTIP);

            _commandBarButtonSolutionExplorerAnalyzeSolution = CreateCommandBarButton(
                _addInInstance, SolutionExplorerSolutionPopup.CommandBar,
                 COMMAND_ANALYSE_SOLUTION_NAME, COMMAND_ANALYSE_SOLUTION_CAPTION, COMMAND_ANALYSE_SOLUTION_TOOLTIP);
        }

        private void setupStop()
        {
            _commandBarButtonMenuStop = CreateCommandBarButton(
                       _addInInstance, CommandBarPopupAddIn.CommandBar,
                       COMMAND_STOP_NAME, COMMAND_STOP_CAPTION, COMMAND_STOP_TOOLTIP);

            _commandBarButtonMenuStop.BeginGroup = true;

            _commandBarButtonCodeStop = CreateCommandBarButton(
                       _addInInstance, CodePopupAddIn.CommandBar,
                       COMMAND_STOP_NAME, COMMAND_STOP_CAPTION, COMMAND_STOP_TOOLTIP);

            _commandBarButtonCodeStop.BeginGroup = true;

            _commandBarButtonSolutionExplorerStop = CreateCommandBarButton(
                       _addInInstance, SolutionExplorerSolutionPopup.CommandBar,
                       COMMAND_STOP_NAME, COMMAND_STOP_CAPTION, COMMAND_STOP_TOOLTIP);

            _commandBarButtonSolutionExplorerStop.BeginGroup = true;
        }

        private void setupOpenLog()
        {
            _commandBarButtonMenuOpenLog = CreateCommandBarButton(
                       _addInInstance, CommandBarPopupAddIn.CommandBar,
                       COMMAND_OPENLOG_NAME, COMMAND_OPENLOG_CAPTION, COMMAND_OPENLOG_CAPTION);

            _commandBarButtonCodeOpenLog = CreateCommandBarButton(
                      _addInInstance, CodePopupAddIn.CommandBar,
                      COMMAND_OPENLOG_NAME, COMMAND_OPENLOG_CAPTION, COMMAND_OPENLOG_CAPTION);

            _commandBarButtonSolutionExplorerOpenLog = CreateCommandBarButton(
                      _addInInstance, SolutionExplorerSolutionPopup.CommandBar,
                      COMMAND_OPENLOG_NAME, COMMAND_OPENLOG_CAPTION, COMMAND_OPENLOG_CAPTION);
        }

        private void setupSettings()
        {
            _commandBarButtonMenuSettings = CreateCommandBarButton(
                           _addInInstance, CommandBarPopupAddIn.CommandBar,
                           COMMAND_SETTINGS_NAME, COMMAND_SETTINGS_CAPTION, COMMAND_SETTINGS_TOOLTIP);

            _commandBarButtonCodeSettings = CreateCommandBarButton(
                           _addInInstance, CodePopupAddIn.CommandBar,
                           COMMAND_SETTINGS_NAME, COMMAND_SETTINGS_CAPTION, COMMAND_SETTINGS_TOOLTIP);

            _commandBarButtonSolutionExplorerSettings = CreateCommandBarButton(
                           _addInInstance, SolutionExplorerSolutionPopup.CommandBar,
                           COMMAND_SETTINGS_NAME, COMMAND_SETTINGS_CAPTION, COMMAND_SETTINGS_TOOLTIP);
        }

        private void setupAbout()
        {
            _commandBarButtonMenuAbout = CreateCommandBarButton(
                       _addInInstance, CommandBarPopupAddIn.CommandBar,
                       COMMAND_ABOUT_NAME, COMMAND_ABOUT_CAPTION, COMMAND_ABOUT_TOOLTIP);

            _commandBarButtonCodeAbout = CreateCommandBarButton(
                _addInInstance, CodePopupAddIn.CommandBar,
                COMMAND_ABOUT_NAME, COMMAND_ABOUT_CAPTION, COMMAND_ABOUT_TOOLTIP);

            _commandBarButtonSolutionExplorerAbout = CreateCommandBarButton(
                _addInInstance, SolutionExplorerSolutionPopup.CommandBar,
                 COMMAND_ABOUT_NAME, COMMAND_ABOUT_CAPTION, COMMAND_ABOUT_TOOLTIP);
        }

        private void ShowErrorView()
        {
            const string ERRORVIEW_WINDOW_GUID = "{3686F36E-2886-4B63-B567-8B9FCC42F392}";

            EnvDTE80.Windows2 windows2;
            string assembly;
            object errorViewControlObject = null;
            ErrorViewUserControl errorViewUserControl;

            try
            {
                if (_windowErrorView == null) // First time, create it
                {
                    windows2 = (EnvDTE80.Windows2)_applicationObject.Windows;
                    
                    assembly = System.Reflection.Assembly.GetExecutingAssembly().Location;

                    _windowErrorView = windows2.CreateToolWindow2(_addInInstance, assembly,
                       typeof(ErrorViewUserControl).FullName, "CppFixIt Error View", ERRORVIEW_WINDOW_GUID, ref errorViewControlObject);

                    errorViewUserControl = (ErrorViewUserControl)errorViewControlObject;

                    // Now you can pass values to the instance of the usercontrol
                    // myUserControl.Initialize(value1, value2)

                }

                _windowErrorView.Visible = true;
                _windowErrorView.IsFloating = false;
            }
            catch (System.Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.ToString());
            }

        }

       
        #endregion
	}
}