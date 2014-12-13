using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.VCProjectEngine;
using CppFixItAddIn;
using System.Reflection;
using System.Configuration;
using System.Text.RegularExpressions;

namespace CppFixItAddIn
{
    internal class ClangAnalyzer
    {
        private static TraceSource ts = Tracer.Instance.CreateTraceSource("ClangAnalyzer");
        private readonly string outputWindowName = "CppFixIt";

        public string ProcessPath
        {
            get
            {
                string processName;
                string processNameConfig = Settings.Default.ClangExeFullPath;
                if (String.IsNullOrEmpty(processNameConfig))
                {
                    processName = GetProcessPathDefault();
                    //ts.TraceInformation("GetProcessPath default " + processName);
                }
                else
                {
                    //ts.TraceInformation("GetProcessPath in settings: " + processNameConfig);
                    processName = processNameConfig;
                }

                return processName;
            }
        }

        private System.Diagnostics.Process _process;
        private Reporting.Report _report;

        #region VS
        private readonly DTE2 _dte2;
        private OutputWindowPane _outputWindowPane;
        private Window _window;
        #endregion

        #region Delegates
        public delegate void AnalyzerDelegate(bool success);
        public AnalyzerDelegate DelegateStartAnalyze;
        public AnalyzerDelegate DelegateStopAnalyze;
        public AnalyzerDelegate DelegateStartFile;
        public AnalyzerDelegate DelegateStopFile;
        public AnalyzerDelegate DelegateStartProject;
        public AnalyzerDelegate DelegateStopProject;
        public AnalyzerDelegate DelegateStartSolution;
        public AnalyzerDelegate DelegateStopSolution;

        public delegate void ViolationDelegate(Reporting.Violation violation);

        public ViolationDelegate DelegateViolation;

        #endregion

        public enum AnalyzeState { IDLE, RUNNING }
        enum AnalyzeType { FILE, PROJECT, SOLUTION };

        public AnalyzeState State {get;set;}

        AnalyzeType _analyzeType;
        VCFile _vcFile;
        VCProject _vcProject;
        Solution _solution;
        VCConfiguration _vcConfiguration;

        Queue<VCFile> _vcFileQueue;
        Queue<VCProject> _vcProjectQueue;

        static public Regex RegExFileLineColumnMessage = new Regex(@"(\D.+)\((\d+),(\d+)\)\s:\s+(warning|error|fatal\serror):\s(.+)"); 

        public ClangAnalyzer(DTE2 dte2)
        {
            State = AnalyzeState.IDLE;
            _report = new Reporting.Report();
            _dte2 = dte2;
            SetOutputPaneWindows();
        }

        static public string GetProcessPathDefault()
        {
            return Path.Combine(new string[] {
                     Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CppFixIt", "bin", "clang.exe" });
        }

        static public string GetProcessPath()
        {
            string processName;
            string processNameConfig = Settings.Default.ClangExeFullPath;
            if (String.IsNullOrEmpty(processNameConfig))
            {
                processName = GetProcessPathDefault();
                Settings.Default.ClangExeFullPath = processNameConfig;
                ts.TraceInformation("GetProcessPath default " + processName);
            }
            else
            {
                ts.TraceInformation("GetProcessPath in settings: " + processNameConfig);
                
                processName = processNameConfig;
            }

            return processName;
        }

        private void CheckProcess()
        {
            string processPath = GetProcessPath();
            if (File.Exists(processPath) == false)
            {
                ts.TraceData(TraceEventType.Error, 1, "analyzer does not exist, cannot found " + processPath);
                throw new SystemException(processPath + " analyzer does not exist");
            }
        }

        public void Stop()
        {
            OutputPaneWriteln("Stopping analysis ...");
            switch (State)
            {
                case AnalyzeState.IDLE:
                    ts.TraceInformation("Stop not running anyway");
                    break;
                case AnalyzeState.RUNNING:
                    if (_process != null)
                    {
                        ts.TraceInformation("Stop killing process");
                        _process.Kill();
                        State = AnalyzeState.IDLE;
                    }
                    else
                    {
                        ts.TraceInformation("Stop running but no process to kill");
                        OnStopAnalyze(true);
                    }
                    break;
                default:
                    ts.TraceData(TraceEventType.Error, 1, "Stop not in a know state");
                    break;
            }
        }

        public void AnalyzeSolution(Solution solution)
        {
            try
            {
                _solution = solution;
                ts.TraceInformation("AnalyzeSolution " + solution.FullName);
                OnStartAnalyze();
                OnStartSolution();

                OutputPaneWriteln("Analyzing solution " + solution.FullName);
                Projects projects = solution.Projects;
                if ((projects == null) || (projects.Count == 0))
                {
                    ts.TraceInformation("No projects in solution");
                    OnStopAnalyze(true);
                    return;
                }

                ts.TraceInformation("AnalyzeSolution #projects " + projects.Count);

                _vcProjectQueue = DTE2Utils.CreateVCProjectQueue(solution);

                _report = Reporting.ReportFactory.CreateReportFromVsSolution(solution, _vcProjectQueue);

                OutputPaneWriteln("Analyzing solution " + solution.FullName +
                    ", " + _report.SolutionCurrent.NumberOfProject + " projects" + 
                    ", " + _report.SolutionCurrent.NumberOfFiles + " files");

                ts.TraceInformation("AnalyzeSolution " + _vcProjectQueue.Count + " c++ projects " +
                    " in " + _report.SolutionCurrent.NumberOfFiles + " files");

                if (_vcProjectQueue.Count == 0)
                {
                    ts.TraceInformation("AnalyzeSolution no c++ project found " + solution.Projects.Count);
                    OnStopAnalyze(true);
                }

                _analyzeType = AnalyzeType.SOLUTION;
                AnalyseNextProject();
            }
            catch (Exception exception)
            {
                ts.TraceData(TraceEventType.Error, 1, "AnalyzeSolution exception: " + exception.Message);
                OnStopSolution(true);
            }
        }

        public void AnalyzeProject(Solution solution, VCProject vcProject)
        {
            try
            {
                ts.TraceInformation("AnalyzeProject " + vcProject.Name);
                _analyzeType = AnalyzeType.PROJECT;
                _vcProject = vcProject;

                _report = Reporting.ReportFactory.CreateReportForVCProject(solution, vcProject);

                OnStartAnalyze();
                DoAnalyzeProject(vcProject);
            }
            
            catch (Exception exception)
            {
                ts.TraceData(TraceEventType.Error, 1, "AnalyzeProject exception: " + exception.Message);
                OnStopProject(true);
            }
        }

        public void AnalyzeFile(Solution solution, VCFile vcFile, VCConfiguration vcConfiguration)
        {
            try
            {
                ts.TraceInformation("AnalyzeFile");
                ts.TraceInformation("AnalyzeFile name " + vcFile.Name);
                ts.TraceInformation("AnalyzeFile item name " + vcFile.ItemName);
                ts.TraceInformation("AnalyzeFile item type " + vcFile.ItemType);
                ts.TraceInformation("AnalyzeFile extension " + vcFile.Extension);
                ts.TraceInformation("AnalyzeFile config name " + vcConfiguration.ConfigurationName);
                ts.TraceInformation("AnalyzeFile config type " + vcConfiguration.ConfigurationType);
                _analyzeType = AnalyzeType.FILE;

                _solution = solution;
                _vcFile = vcFile;
                _vcConfiguration = vcConfiguration;
                _report = Reporting.ReportFactory.CreateReportSingleFile(solution, vcFile);

                OnStartAnalyze();
                DoAnalyzeFile(vcFile, vcConfiguration);
            }

            catch (Exception exception)
            {
                ts.TraceData(TraceEventType.Error, 1, "AnalyzeFile exception: " + exception.Message);
                OnStopFile(true);
            }
        }

        private void DoAnalyzeProject(VCProject vcProject)
        {
            ts.TraceInformation("DoAnalyzeProject " + vcProject.Name);
            ts.TraceInformation("DoAnalyzeProject #files " + _report.SolutionCurrent.ProjectCurrent.Files.Count);
            OutputPaneWriteln("Analyzing project " + vcProject.Name +
                " " + _report.SolutionCurrent.ProjectCurrent.Files.Count + " files to analyze");

            OnStartProject();

            ts.TraceInformation("DoAnalyzeProject " + vcProject.Name + " getting vc conf");
            _vcConfiguration = DTE2Utils.GetVcConfiguratioForVcProject(vcProject);
            if (_vcConfiguration == null)
            {
                ts.TraceInformation("DoAnalyzeProject cannot get c++ project configuration" + vcProject.Name);
                OnStopProject(true);
                return;
            }

            ts.TraceInformation("DoAnalyzeProject " + vcProject.Name + " create file queue");
            _vcProject = vcProject;
            _vcFileQueue = DTE2Utils.CreateFileQueue(vcProject);
            ts.TraceInformation("DoAnalyzeProject " + vcProject.Name + " file queue created");

            if (_vcFileQueue.Count == 0)
            {
                ts.TraceInformation("DoAnalyzeProject no c++ file in project " + vcProject.Name);
                OnStopProject(false);
                return;
            }

            AnalyseNextFile();
        }

        private void DoAnalyzeFile(VCFile vcFile, VCConfiguration vcConfiguration)
        {
            _vcFile = vcFile;

            ts.TraceInformation("DoAnalyze " + vcFile.Name);
            ts.TraceInformation("DoAnalyze configuration name " + vcConfiguration.Name);
            OnStartFile();
            OutputPaneWriteln("Analyze " + vcFile.Name);

            try
            {
                ProcessStart(ProcessPath, GetProcessArgument(vcFile, vcConfiguration), vcFile.project.ProjectDirectory);
            }
            catch (Exception exception)
            {
                ts.TraceData(TraceEventType.Error, 1, "DoAnalyze cannot start analyzer process" + exception.Message);
                OutputPaneWriteln("Cannot start analyser: " + exception.Message);
                OnStopFile(true);
            }

            ts.TraceInformation("DoAnalyze analyzing");
        }



        private void AnalyseNextFile()
        {
            ts.TraceInformation("AnalyseNextFile project " + _vcProject.Name +  ", #files still to be processed " + _vcFileQueue.Count);
            if (_vcFileQueue.Count == 0)
            {
                OnStopProject(false);
            }
            else
            {
                VCFile vcFile = _vcFileQueue.Dequeue();
                _report.NextFile();
                DoAnalyzeFile(vcFile, _vcConfiguration);
            }
        }

        private void AnalyseNextProject()
        {
            ts.TraceInformation("AnalyseNextProject #project still to be processed " + _vcProjectQueue.Count);
            if (_vcProjectQueue.Count == 0)
            {
                OnStopSolution(false);
            }
            else
            {
                VCProject vcProject = _vcProjectQueue.Dequeue();
                
                _report.NextProject();
                ts.TraceInformation("AnalyseNextProject " + _report.ProjectCurrent.Name);
                ts.TraceInformation("AnalyseNextProject " + vcProject.Name);
                DoAnalyzeProject(vcProject);
            }
        }

        #region On start and stop

        private void OnStartFile()
        {
            ts.TraceInformation("OnStartFile");
            DelegateStartFile(true);
        }

        private void OnStartProject()
        {
            ts.TraceInformation("OnStartProject");
            DelegateStartProject(true);
        }

        private void OnStartSolution()
        {
            ts.TraceInformation("OnStartSolution");
            DelegateStartSolution(true);
        }

        private void OnStartAnalyze()
        {
            ts.TraceInformation("OnStartAnalyze");
            State = AnalyzeState.RUNNING;
            OutputPaneClear();
            OutputPaneShow();
            ReportingStart();
            DelegateStartAnalyze(true);
        }

        private void OnStopFile(bool error)
        {
            ts.TraceInformation("OnStopFile " + _vcFile.Name);
            OutputPaneWriteln("Analyzed file " + _vcFile.Name);

            Reporting.File file = _report.FileCurrent;
            OutputPaneWriteln("#warning " + file.Violations.Count);

            DelegateStopFile(error);
            _process = null;

            switch (State)
            {
                case AnalyzeState.IDLE:
                    ts.TraceInformation("OnStopFile stopping on user's request");
                    OnStopAnalyze(error);
                    break;
                case AnalyzeState.RUNNING:
                    if (error)
                    {
                        OnStopAnalyze(error);
                    }
                    else
                    {

                        switch (_analyzeType)
                        {
                            case AnalyzeType.FILE:
                                OnStopAnalyze(error);
                                break;
                            case AnalyzeType.PROJECT:
                                AnalyseNextFile();
                                break;
                            case AnalyzeType.SOLUTION:
                                AnalyseNextFile();
                                break;
                            default:
                                break;
                        }
                    }
                    break;
                default:
                    ts.TraceData(TraceEventType.Error, 1, "Stop not in a know state");
                    break;
            }

       
        }

        private void OnStopProject(bool error)
        {
            ts.TraceInformation("OnStopProject");
            OutputPaneWriteln("Analyzed project " + _vcProject.Name + " #files " +  _report.ProjectCurrent.Files.Count);
            OutputPaneWriteln("\n" + String.Empty.PadLeft(128, '-'));

            DelegateStopProject(error);
            switch (_analyzeType)
            {
                case AnalyzeType.FILE:
                    //Should never happen
                    break;
                case AnalyzeType.PROJECT:
                    OnStopAnalyze(error);
                    break;
                case AnalyzeType.SOLUTION:
                    AnalyseNextProject();
                    break;
                default:
                    break;
            }
        }

        private void OnStopSolution(bool error)
        {
            ts.TraceInformation("OnStopSolution");
            OutputPaneWriteln("Analyzed solution " + _solution.FullName);
            if (_report != null)
            {
                OutputPaneWriteln("warnings: " + _report.SolutionCurrent.ViolationCount);
            }

            DelegateStopSolution(error);
            switch (_analyzeType)
            {
                case AnalyzeType.FILE:
                    //Should never happen
                    break;
                case AnalyzeType.PROJECT:
                    //Should never happen
                    break;
                case AnalyzeType.SOLUTION:
                    OnStopAnalyze(error);
                    break;
                default:
                    break;
            }
        }

        private void OnStopAnalyze(bool error)
        {
            ts.TraceInformation("OnStopAnalyze");
            State = AnalyzeState.IDLE;

            ReportingStop();

            if (error)
            {
                OutputPaneWriteln("************* Analyzing errors, see log for more info ");

            }
            else
            {
                ReportDisplay();
                OutputPaneWriteln("Analyzing done");
            }

            OutputPaneShow();
            
            DelegateStopAnalyze(true);
        }

        private void ReportDisplay()
        {
            ts.TraceInformation("ReportDisplay");
            if (_report == null)
            {
                return;
            }

            switch (_analyzeType)
            {
                case AnalyzeType.FILE:
                    //TODO
                    break;
                case AnalyzeType.PROJECT:
                    OutputPaneWriteln(Reporting.ReporterText.CreateDisplay(_report.SolutionCurrent.ProjectCurrent));
                    break;
                case AnalyzeType.SOLUTION:
                    OutputPaneWriteln(Reporting.ReporterText.CreateDisplay(_report.SolutionCurrent));
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region Reporting

        private void ReportingStart()
        {
             ts.TraceInformation("ReportingStart");
        }

        private void ReportingStop()
        {
             ts.TraceInformation("ReportingStop");
        }
        #endregion

        #region Process

        private string GetReportFile()
        {
            return "report.xml";
        }

        private String GetProcessArgument(VCFile vcFile, VCConfiguration vcConfiguration)
        {
            VCFileConfiguration vcFileConfiguration = DTE2Utils.GetVcFileConfiguration(vcFile, vcConfiguration);
            if (vcFileConfiguration == null)
            {
                ts.TraceData(TraceEventType.Error, 1, "GetProcessArgument cannot get VcFileConfiguration ");
                return "";
            }

            StringBuilder stringBuilder = new StringBuilder();
            //stringBuilder.Append(" -Xclang -analyzer-display-progress ");
            stringBuilder.Append(" --analyze ");
            

            //stringBuilder.Append(" -o " + GetReportFile() + " ");

            if (!vcFile.Name.EndsWith(".c"))
            {
                stringBuilder.Append(" -x c++ ");
            }
            stringBuilder.Append(DTE2Utils.GetIncludesForProject(vcConfiguration));
            stringBuilder.Append(DTE2Utils.GetDefinesForProject(vcConfiguration));
            stringBuilder.Append(DTE2Utils.GetIncludesForFile(vcFileConfiguration));
            stringBuilder.Append(DTE2Utils.GetDefinesForFile(vcFileConfiguration));
            stringBuilder.Append(GetArgumentMicrosoftCompilerSpecific());
            
            stringBuilder.Append("\"" + vcFile.FullPath + "\"");
            return stringBuilder.ToString();
        }

        private String GetArgumentMicrosoftCompilerSpecific()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(" -fmsc-version=1600 ");
            stringBuilder.Append(" -fms-extensions ");
            stringBuilder.Append(" -fms-compatibility ");
            stringBuilder.Append(" -fdiagnostics-format=msvc ");
            return stringBuilder.ToString();
        }

        private void ProcessStart(string name, string argument, string directory)
        {
            // throw if process does not exist
            CheckProcess();

            if (_process == null)
            {
                //directory
                Directory.SetCurrentDirectory(directory);

                ts.TraceInformation("ProcessStart \n" + name + " " + argument + "\n in " + directory);
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.FileName = name;
                process.StartInfo.Arguments = argument;
                process.StartInfo.WorkingDirectory = directory;
                process.EnableRaisingEvents = true;
                process.Exited += new EventHandler(this.ProcessExited);
                process.OutputDataReceived += new DataReceivedEventHandler(this.ProcessOutputDataReceived);
                process.ErrorDataReceived += new DataReceivedEventHandler(this.ProcessErrorDataReceived);
                process.Start();
                process.BeginErrorReadLine();
                process.BeginOutputReadLine();
                _process = process;
            }
            else
            {
                ts.TraceData(TraceEventType.Error, 1, "Process already started");
            }
        }

        private void ProcessExited(object sender, System.EventArgs e)
        {
            ts.TraceInformation("ProcessExited");
            OnStopFile(false);
        }

        private void ProcessOutputDataReceived(object sender, DataReceivedEventArgs outLine)
        {
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                ts.TraceInformation("ProcessOutputDataReceived:\n" + outLine.Data);
                OutputPaneWrite(outLine.Data + "\n");
                
            }
        }

        private void ProcessErrorDataReceived(object sender, DataReceivedEventArgs outLine)
        {
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                ts.TraceInformation("ProcessErrorDataReceived:\n" + outLine.Data);
                OutputPaneWrite(outLine.Data + "\n");
                UpdateRecord(outLine.Data);
            }
        }
        #endregion 

        private void UpdateRecord(string line)
        {
            Match match = RegExFileLineColumnMessage.Match(line);
            string file;
            string lineNumber;
            string columnNumber;
            string type;
            string message;
            ts.TraceData(TraceEventType.Verbose, 1, "UpdateRecord match count " + match.Groups.Count);
            if (match.Groups.Count == 6)
            {
                file = match.Groups[1].Value;
                lineNumber = match.Groups[2].Value;
                columnNumber = match.Groups[3].Value;
                type = match.Groups[4].Value;
                message = match.Groups[5].Value;

                Reporting.Violation violation = _report.CreateViolation();
                violation.FilePath = file;
                violation.LineNumber = lineNumber;
                violation.ColumnNumber = columnNumber;
                if (type == "warning")
                {
                    violation.Severity = Reporting.Violation.ESeverity.WARNING;
                }
                else 
                {
                    violation.Severity = Reporting.Violation.ESeverity.ERROR;
                }

                violation.Message = message;
                violation.FullMessage = line;
                if (_report.AddViolation(violation))
                {
                    DelegateViolation(violation);
                }
                else
                {
                    ts.TraceData(TraceEventType.Verbose, 1, "UpdateRecord already added");
                }
            }
        }

        #region OutputPane

        private void SetOutputPaneWindows()
        {
            _window = _dte2.Windows.Item(Constants.vsWindowKindOutput);
            _window.Visible = true;

            var outputWindow = (OutputWindow)_window.Object as OutputWindow;

            try
            {
                _outputWindowPane = outputWindow.OutputWindowPanes.Item(outputWindowName);
            }
            catch
            {
                _outputWindowPane = outputWindow.OutputWindowPanes.Add(outputWindowName);
            }
        }

        private void OutputPaneWrite(string text)
        {
            //ts.TraceData(TraceEventType.Verbose, 1, "OutputPaneClear " + text);
            _outputWindowPane.OutputString(text);
        }

        private void OutputPaneWriteln(string text)
        {
            _outputWindowPane.OutputString(text + "\n");
        }

        private void OutputPaneClear()
        {
            ts.TraceData(TraceEventType.Verbose, 1, "OutputPaneClear");
            _outputWindowPane.Clear();
        }

        private void OutputPaneShow()
        {
            ts.TraceData(TraceEventType.Verbose, 1, "OutputPaneShow");
            _window.Activate();
            _outputWindowPane.Activate();
            Application.DoEvents();
        }
        #endregion

        
    }
}
