using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Reflection;
using System.IO;
using Extensibility;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.VCProject;
using Microsoft.VisualStudio.VCProjectEngine;

namespace CppFixItAddIn
{
    class DTE2Utils
    {
        private static TraceSource ts = Tracer.Instance.CreateTraceSource("DTE2Utils");

        private DTE2Utils() { }

        public enum ProjectType
        {
            UNKNOWN,
            CPP_PROJECT,
            CSHARP_PROJECT
        }

        public static bool HasVcStartupProject(Solution solution)
        {
            VCProject vcProject = DTE2Utils.GetVcProjectStartup(solution);
            if (vcProject == null)
            {
                //ts.TraceInformation("HasVcStartupProject:  cannot get startup vc project from solution");
                return false;
            }
            else
            {
                return true;
            }
        }

        public static bool IsVcProjectSelected(DTE2 dte2)
        {
            try
            {
                Solution solution = dte2.Solution;
                Array projects = (Array)dte2.ActiveSolutionProjects;
                if ((projects != null) && (projects.Length > 0))
                {
                    Project project = (Project)projects.GetValue(0);
                    VCProject vcProject = project.Object as VCProject;
                    if (vcProject != null)
                    {
                        return true;
                    }
                }
            }
            catch (Exception)
            {
 
            }
            return false;
        }

        public static bool HasVcProjectsInSolution(Solution solution)
        {
            return (CreateVCProjectQueue(solution).Count > 0);
        }

        static public VCProject GetVcProjectStartup(Solution solution)
        {
            ts.TraceData(TraceEventType.Verbose, 1, "GetVcProjectStartup");
            if ((solution == null) || solution.SolutionBuild == null || (solution.SolutionBuild.StartupProjects == null))
            {
                ts.TraceData(TraceEventType.Verbose, 1, "GetVcProjectStartup no one found");
                return null;
            }

            Array startupProjects = solution.SolutionBuild.StartupProjects as Array;
            string startupProjectName = startupProjects.GetValue(0) as string;

            Queue<VCProject> vcProjectQueue = CreateVCProjectQueue(solution);
            foreach (VCProject vcProject in vcProjectQueue)
            {
                Project project = vcProject.Object as Project;
                if (project != null)
                {
                    ts.TraceData(TraceEventType.Verbose, 1, "GetVcProjectStartup " + project.Name);
                    if (project.FullName.EndsWith(startupProjectName))
                    {
                        ts.TraceData(TraceEventType.Verbose, 1, "GetVcProjectStartup found " + startupProjectName);
                        return vcProject;
                    }
                }
            }

            ts.TraceData(TraceEventType.Verbose, 1, "GetVcProjectStartup cannot find c++ startup project");
            return null;
        }

        static public bool HasVcCppFileActive(Document document)
        {
            return (GetVcCppFile(document) != null);
        }

        static public VCFile GetVcCppFile(Document document)
        {
            if (document == null)
            {
                ts.TraceData(TraceEventType.Verbose, 1, "GetCppFile no active document");
                return null;
            }

            ts.TraceData(TraceEventType.Verbose, 1, "GetCppFile full name " + document.FullName);

            if (document.Language != "C/C++")
            {
                ts.TraceData(TraceEventType.Verbose, 1, "GetCppFile not a C/C++ but " + document.Language);
                return null;
            }

            ProjectItem projectItem = document.ProjectItem;
            VCFile vcFile = projectItem.Object as VCFile;
            if (vcFile == null)
            {
                ts.TraceData(TraceEventType.Verbose, 1, "GetCppFile not a VCFile");
                return null;
            }

            if (vcFile.FileType != eFileType.eFileTypeCppCode)
            {
                ts.TraceData(TraceEventType.Verbose, 1, "GetCppFile not a C++ file : " + vcFile.FileType);
                return null;
            }

            ts.TraceData(TraceEventType.Verbose, 1, "GetCppFile found cpp");
            return vcFile;
        }

        static public Configuration GetConfigurationActive(Document document)
        {
            if (document != null)
            {
                return document.ProjectItem.ConfigurationManager.ActiveConfiguration;
            }
            else
            {
                ts.TraceData(TraceEventType.Error, 1, "GetConfigurationActive no active document");
                return null;
            }
        }

        static public void SaveCurrentDocument(Document document, VCFile vcFile)
        {
            ts.TraceInformation("Save " + vcFile.FullPath);
            if (!document.Saved)
            {
                document.Save(vcFile.FullPath);
            }
        }
        static public VCConfiguration GetVcConfiguratioForVcProject(VCProject vcProject)
        {
            ts.TraceData(TraceEventType.Verbose, 1, "GetVcConfiguratioForVcProject project " + vcProject.Name);
            Project project = vcProject.Object as Project;
            if ((project == null) || (project.ConfigurationManager == null))
            {
                return null;
            }

            Configuration configuration = project.ConfigurationManager.ActiveConfiguration;
            if (configuration == null)
            {
                return null;
            }

            ts.TraceData(TraceEventType.Verbose, 1, "GetVcConfiguration configuration name " + configuration.ConfigurationName);
            return GetVcConfiguration(vcProject, configuration);
        }

        static public VCConfiguration GetVcConfigurationForDocument(Document document, VCProject vcProject)
        {
            if (vcProject == null)
            {
                ts.TraceData(TraceEventType.Error, 1, "GetVCConfiguration cannot get vcProject");
                return null;
            }
            
            Configuration configurationActive = GetConfigurationActive(document);
            if (configurationActive == null)
            {
                ts.TraceData(TraceEventType.Error, 1, "GetVCConfiguration cannot get active configuration");
                return null;
            }

            return GetVcConfiguration(vcProject, configurationActive);
        }


        static private VCConfiguration GetVcConfiguration(VCProject vcProject, Configuration configuration)
        {
            ts.TraceData(TraceEventType.Verbose, 1, "GetVcConfiguration " + vcProject.Name);
            if (configuration == null)
            {
                return null;
            }

            try
            {
                var ivCCollection = vcProject.Configurations as IVCCollection;
                if (ivCCollection == null)
                {
                    ts.TraceData(TraceEventType.Error, 1, "GetVCConfiguration cannot get configurations");
                    return null;
                }

                foreach (VCConfiguration vcConfiguration in ivCCollection)
                {
                    if (vcConfiguration.ConfigurationName == configuration.ConfigurationName &&
                        vcConfiguration.Platform.Name == configuration.PlatformName)
                    {
                        ts.TraceData(TraceEventType.Verbose, 1, "GetVCConfiguration found configuration name "
                            + configuration.ConfigurationName + " on platform " + configuration.PlatformName);
                        return vcConfiguration;
                    }
                }
            }
            catch (Exception exception)
            {
                ts.TraceData(TraceEventType.Error, 1, "GetVCConfiguration error: " + exception);
            }

            return null;
        }

        static public VCFileConfiguration GetVcFileConfiguration(VCFile vcFile, VCConfiguration vcConfiguration)
        {
            if (vcConfiguration == null)
            {
                ts.TraceData(TraceEventType.Error, 1, "GetVcFileConfiguration cannot get vc config");
                return null;
            }

            try
            {
                IVCCollection configs = (IVCCollection)vcFile.FileConfigurations;
                if (configs == null) {
                    ts.TraceData(TraceEventType.Error, 1, "GetVcFileConfiguration cannot get configs");
                    return null;
                }

                foreach (VCFileConfiguration vcFileConfiguration in configs)
                {
                    if (vcFileConfiguration.ProjectConfiguration.ConfigurationName == vcConfiguration.ConfigurationName &&
                        vcFileConfiguration.ProjectConfiguration.Platform.Name == vcConfiguration.Platform.Name)
                    {
                        ts.TraceData(TraceEventType.Verbose, 1, "GetVcFileConfiguration found configuration name ");
                        return vcFileConfiguration;
                    }
                }
            }
            catch (Exception exception)
            {
                ts.TraceData(TraceEventType.Error, 1, "GetVcFileConfiguration error: " + exception);
                return null;
            }

            ts.TraceData(TraceEventType.Error, 1, "GetVcFileConfiguration cannot find configuration");
            return null;
        }

        static public string GetIncludesForProject(VCConfiguration vcConfiguration)
        {
            StringBuilder includes = new StringBuilder();
            VCCLCompilerTool vcCTool = GetVCCLCompilerToolForProject(vcConfiguration);

            if (String.IsNullOrEmpty(vcCTool.FullIncludePath) == false)
            {
                ts.TraceInformation("GetIncludesForProject: FullIncludePath\n" + vcCTool.FullIncludePath);
                includes.Append(BuildIncludes(vcCTool.FullIncludePath, "-I"));
            }

            if (String.IsNullOrEmpty(vcCTool.AdditionalIncludeDirectories) == false)
            {
                ts.TraceInformation("GetIncludesForProject: AdditionalIncludeDirectories\n" + vcCTool.AdditionalIncludeDirectories);
                includes.Append(BuildIncludes(vcConfiguration.Evaluate(vcCTool.AdditionalIncludeDirectories), "-I"));
            }

            if (String.IsNullOrEmpty(vcCTool.ForcedIncludeFiles) == false)
            {
                ts.TraceInformation("GetIncludesForProject: ForcedIncludeFiles\n" + vcCTool.ForcedIncludeFiles);
                //TODO HEEM
                includes.Append(BuildIncludes(vcCTool.ForcedIncludeFiles, "-include"));
            }
            
            ts.TraceInformation("GetIncludesForProject: \n" + includes.ToString());

            return includes.ToString();
        }

        static public string GetIncludesForFile(VCFileConfiguration vcFileConfiguration)
        {
            StringBuilder includes = new StringBuilder();
            VCCLCompilerTool vcCTool = (VCCLCompilerTool)vcFileConfiguration.Tool;
            if (String.IsNullOrEmpty(vcCTool.AdditionalIncludeDirectories) == false)
            {
                ts.TraceInformation("GetIncludesForFile: AdditionalIncludeDirectories\n" + vcCTool.AdditionalIncludeDirectories);
                includes.Append(BuildIncludes(vcFileConfiguration.Evaluate(vcCTool.AdditionalIncludeDirectories), "-D"));
            }

            if (String.IsNullOrEmpty(vcCTool.ForcedIncludeFiles) == false)
            {
                ts.TraceInformation("GetIncludesForFile: ForcedIncludeFiles\n" + vcCTool.ForcedIncludeFiles);
                includes.Append(BuildIncludes(vcCTool.ForcedIncludeFiles, "-include"));
            }

            ts.TraceInformation("GetIncludesForFile: \n" + includes.ToString());
            return includes.ToString();
        }

        static public string GetDefinesForProject(VCConfiguration vcConfiguration)
        {
            StringBuilder defines = new StringBuilder();
            VCCLCompilerTool vcCTool = GetVCCLCompilerToolForProject(vcConfiguration);

            if (String.IsNullOrEmpty(vcCTool.PreprocessorDefinitions) == false)
            {
                ts.TraceInformation("GetDefinesForProject: PreprocessorDefinitions\n" + vcCTool.PreprocessorDefinitions);
                defines.Append(BuildDefines(vcCTool.PreprocessorDefinitions));
            }

            switch (vcCTool.RuntimeLibrary)
            {
              case runtimeLibraryOption.rtMultiThreaded:
                defines.Append("-D_MT ");
                break;
              case runtimeLibraryOption.rtMultiThreadedDebug:
                defines.Append("-D_MT -D_DEBUG ");
                break;
              case runtimeLibraryOption.rtMultiThreadedDLL:
                defines.Append("-D_MT -D_DLL ");
                break;
              case runtimeLibraryOption.rtMultiThreadedDebugDLL:
                defines.Append("-D_MT -D_DLL -D_DEBUG ");
                break;
            }

            ts.TraceInformation("GetDefinesForProject:  " + defines.ToString());
            return defines.ToString();
        }

        static public string GetDefinesForFile(VCFileConfiguration vcFileConfiguration)
        {
            StringBuilder defines = new StringBuilder();
            VCCLCompilerTool vcCTool = (VCCLCompilerTool)vcFileConfiguration.Tool;

            if (String.IsNullOrEmpty(vcCTool.PreprocessorDefinitions) == false)
            {
                ts.TraceInformation("GetDefinesForFile: PreprocessorDefinitions\n" + vcCTool.PreprocessorDefinitions);
                defines.Append(BuildDefines(vcCTool.PreprocessorDefinitions));
            }

            ts.TraceInformation("GetDefinesForFile PreprocessorDefinitions:\n" + vcCTool.PreprocessorDefinitions);
            return defines.ToString();
        }

        static private string BuildIncludes(string rawIncludes, string includeType)
        {
            StringBuilder includes = new StringBuilder();
            String cIncDir = rawIncludes.Replace("\"", "").Replace(",", ";");
            String[] cIncDirs = cIncDir.Split(';');

            var uniqueDirs = new HashSet<String>();
            foreach (string inc in cIncDirs)
            {
                if (inc.Length > 0)
                {
                    String incCheck = inc.Replace("\\", "/");
                    if (incCheck == "./") incCheck = ".";
                    if (incCheck == "../") incCheck = "..";

                    // resolve any relative paths
                    incCheck = Path.GetFullPath(incCheck);
                    //incCheck = Path.GetFullPath(Path.Combine(baseDir,incCheck));
                    //TODO HEEM do we need that ?
                    if (IsSystemInclude(incCheck) == false)
                    {
                        incCheck = incCheck.Replace("\\", "/");
                    }
                    uniqueDirs.Add(incCheck);
                }
            }

            foreach (String inc in uniqueDirs)
            {
                if (IsSystemInclude(inc) == false)
                {
                    // -I or -include
                    includes.Append(includeType + " \"");
                }
                else
                {
                    includes.Append("-isystem " + " \"");
                }

                includes.Append(inc);
                includes.Append("\" ");
            }

            return includes.ToString();
        }

        static private string BuildDefines(string rawDefine)
        {
            StringBuilder defines = new StringBuilder();
            String cPDefine = rawDefine.Replace(",", ";");
            String[] cPDefines = cPDefine.Split(';');

            foreach (string pd in cPDefines)
            {
                if (pd.Length > 0)
                {
                    defines.Append("-D");
                    defines.Append(pd);
                    defines.Append(" ");
                }
            }

            return defines.ToString();
        }

        static private bool IsSystemInclude(string include)
        {
            if (include.Contains("Microsoft Visual Studio")
                || include.Contains("Microsoft SDKs"))
            {
                //ts.TraceInformation("IsSystemInclude " + include);
                return true;
            }
            else
            {
                return false;
            }
        }

        static private VCCLCompilerTool GetVCCLCompilerToolForProject(VCConfiguration vcConfiguration)
        {
            IVCCollection fTools = (IVCCollection)vcConfiguration.Tools;
            VCCLCompilerTool vcCTool = (VCCLCompilerTool)fTools.Item("VCCLCompilerTool");
            return vcCTool;
        }

        /// <summary>
        /// Converts a project type string to an enum.
        /// </summary>
        //static public ProjectType ConvertProjectTypeToEnum(Project project)
        //{
        //    string projectType = project.Kind;
        //    ProjectType result = ProjectType.UNKNOWN;

        //    // The project type is a GUID representing a Visual Studio 
        //    // project type...
        //    switch (projectType)
        //    {
        //        case "{8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942}":
        //            result = ProjectType.CPP_PROJECT;
        //            break;

        //        case "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}":
        //            result = ProjectType.CSHARP_PROJECT;
        //            break;
        //    }

        //    return result;
        //}

        static public Queue<VCProject> CreateVCProjectQueue(Solution solution)
        {
            ts.TraceData(TraceEventType.Verbose, 1, "CreateVCProjectQueue for solution " + solution.FullName);
            Queue<VCProject> vcProjectQueue = new Queue<VCProject>();
            if (solution.Projects != null)
            {
                foreach (Project project in solution.Projects)
                {
                    try
                    {
                        AddProjectQueue(project, ref vcProjectQueue);
                    }
                    catch (Exception exception)
                    {
                        ts.TraceData(TraceEventType.Error, 1, "CreateVCProjectQueue: exception " + exception.Message);
                        ts.TraceData(TraceEventType.Error, 1, exception.StackTrace);
                    }
                }

                
            }
            ts.TraceData(TraceEventType.Verbose, 1, "CreateVCProjectQueue #projects " + vcProjectQueue.Count);
            return vcProjectQueue;
        }

        static private void AddProjectQueue(Project project, ref Queue<VCProject> vcProjectQueue)
        {
            ts.TraceData(TraceEventType.Verbose, 1, "AddProjectQueue for project " + project.Name);

            VCProject vcProject = project.Object as VCProject;
            if (vcProject != null)
            {
                ts.TraceData(TraceEventType.Verbose, 1, "AddProjectQueue add cpp project " + vcProject.Name);
                vcProjectQueue.Enqueue(vcProject);
            }
            else
            {
                ts.TraceData(TraceEventType.Verbose, 1, "AddProjectQueue not a cpp project " + project.Name);
            }

            if (project.ProjectItems != null)
            {
                foreach (ProjectItem projectItem in project.ProjectItems)
                {
                    ts.TraceData(TraceEventType.Verbose, 1, "AddProjectQueue project item " + projectItem.Name);

                    Project subProject = projectItem.SubProject as Project;
                    if (subProject != null)
                    {
                        ts.TraceData(TraceEventType.Verbose, 1, "AddProjectQueue subProject " + subProject.Name);
                        AddProjectQueue(subProject, ref vcProjectQueue);
                    }

                    ProjectItems subItems = projectItem.ProjectItems;

                    if (subItems != null)
                    {
                        ProcessProjectItems(subItems, ref vcProjectQueue);
                    }
                }
            }
        }

        static private void ProcessProjectItems(ProjectItems projectItems, ref Queue<VCProject> vcProjectQueue)
        {
            //ts.TraceInformation("ProcessProjectItems kind: " + projectItems.Kind);
            if ((projectItems == null) || (projectItems.Count == 0))
            {
                return;
            }

            foreach (ProjectItem projectItem in projectItems)
            {
                ProjectItems subItems = projectItem.ProjectItems;

                if (subItems != null)
                {
                    ProcessProjectItems(subItems, ref vcProjectQueue);
                }

                Project subProject = projectItem.SubProject;
                if (subProject != null)
                {
                    AddProjectQueue(subProject, ref vcProjectQueue);
                }
            }
        }

        static public Queue<VCFile> CreateFileQueue(VCProject vcProject)
        {
            ts.TraceInformation("CreateFileQueue");
            Queue<VCFile> fileQueue = new Queue<VCFile>();
            var vcFileCollection = (IVCCollection)vcProject.Files;

            if (vcFileCollection == null)
            {
                ts.TraceData(TraceEventType.Verbose, 1, "CreateFileList cannot get vc file collection");
                return null;
            }

            ts.TraceInformation("CreateFileQueue #vcFileCollection " + vcFileCollection.Count);
            foreach (VCFile vcFile in vcFileCollection)
            {
                try
                {
                    VCFileConfiguration vcFileConfiguration = DTE2Utils.GetVcFileConfiguration(
                        vcFile,
                        DTE2Utils.GetVcConfiguratioForVcProject(vcProject));

                    if (vcFileConfiguration != null)
                    {
                        if ((vcFileConfiguration.ExcludedFromBuild == false) && (vcFile.FileType == eFileType.eFileTypeCppCode))
                        {
                            ts.TraceData(TraceEventType.Verbose, 1, "CreateFileList add " + vcFile.FullPath);
                            fileQueue.Enqueue(vcFile);
                        }
                        else
                        {
                            ts.TraceData(TraceEventType.Verbose, 1, "CreateFileList exclude " + vcFile.FullPath);
                        }
                    }
                }
                catch (Exception exception)
                {
                    ts.TraceData(TraceEventType.Verbose, 1, "CreateFileList exception  " + exception.Message);
                }
            }

            ts.TraceInformation("CreateFileList project " + vcProject.Name  + " #files " + fileQueue.Count);
            return fileQueue;
        }

        static public Project GetProject(VCFile vcFile)
        {
            VCProject vcProject = vcFile.project as VCProject;
            return vcProject.Object as EnvDTE.Project;
        }
    }
}
