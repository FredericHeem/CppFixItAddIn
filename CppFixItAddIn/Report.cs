using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CppFixItAddIn
{
    namespace Reporting
    {
        internal class Violation
        {
            public enum ESeverity { WARNING, ERROR}
            public ESeverity Severity { get; set; }
            public string FilePath { get; set; }
            public string Message { get; set; }
            public string LineNumber { get; set; }
            public string ColumnNumber { get; set; }

            public Violation()
            { 
            }

            public string FullMessage { get; set; }
        }

        class File
        {
            public string Name { get; set; }
            public string FullPath { get; set; }
            public List<Violation> Violations { get; set; }
            public Violation ViolationCurrent;

            public File()
            {
                Violations = new List<Violation>();
            }

            public Violation CreateViolation()
            {
                Violation violation = new Violation();
                ViolationCurrent = violation;
                return violation;
            }
        }

        class Project
        {
            public List<File> Files { get; set; }
            public File FileCurrent { get; set; }
            private int _fileIndex { get; set; }
            public int FileCurrentIndex { get; set; }
            public string Name { get; set; }

            public int ViolationCount {
                get {
                    int violationCount = 0;
                    foreach (File file in Files){
                        violationCount += file.Violations.Count;
                    }

                    return violationCount;
                }
            }

            public int NumberOfFiles
            {
                get {
                   return Files.Count;
                }
            }

            public File CreateFile()
            {
                File file = new File();
                Files.Add(file);
                FileCurrent = file;
                return file;
            }

            public Project()
            {
                Files = new List<File>();
            }

            public void NextFile() {
                _fileIndex++;
                if (_fileIndex <= Files.Count)
                {
                    FileCurrent = Files[_fileIndex - 1];
                }
                else
                {
                    throw new InvalidOperationException("no more file");
                }
            }
        }

        class Solution
        {
            public string Name { get; set; }
            public List<Project> Projects { get; set; }
            public Project _projectCurrent;
            public Project ProjectCurrent
            {
                get
                {
                    if (_projectCurrent == null)
                    {
                        _projectCurrent = CreateProject();
                    }
                    return _projectCurrent;
                }
                set {
                    _projectCurrent = value;
                }
            }

            private int _projectIndex;

            public int ViolationCount
            {
                get
                {
                    int violationCount = 0;
                    foreach (Project project in Projects)
                    {
                        violationCount += project.ViolationCount;
                    }

                    return violationCount;
                }
            }


            public int NumberOfFiles
            {
                get
                {
                    int numberOfFiles = 0;
                    foreach (Project project in Projects)
                    {
                        numberOfFiles += project.NumberOfFiles;
                    }

                    return numberOfFiles;
                }
            }

            public int NumberOfProject
            {
                get
                {
                    return Projects.Count;
                }
            }

            public Solution()
            {
                Projects = new List<Project>();
            }

            public Project CreateProject()
            {
                Project project = new Project();
                Projects.Add(project);
                ProjectCurrent = project;
                return project;
            }

            public void NextProject()
            {
                _projectIndex++;
                if (_projectIndex <= Projects.Count)
                {
                    ProjectCurrent = Projects[_projectIndex - 1];
                }
                else
                {
                    throw new InvalidOperationException("no more project");
                }
            }
        }

        class Report
        {
            public List<Solution> Solutions { get;set;}
            private Dictionary<string,Violation> mapViolation = new Dictionary<string,Violation>();
            private Solution _solutionCurrent;
            public Solution SolutionCurrent
            {
                get {
                    if(_solutionCurrent == null){
                        _solutionCurrent = CreateSolution();
                    }
                    return _solutionCurrent;
                }
                set {
                    _solutionCurrent = value;
                }
            }

            public Project ProjectCurrent
            {
                get {
                    return SolutionCurrent.ProjectCurrent;
                }
                set{}
            }

            public File FileCurrent
            {
                get
                {
                    return SolutionCurrent.ProjectCurrent.FileCurrent;
                }
            }

            public Violation RecordCurrent
            {
                get
                {
                    return SolutionCurrent.ProjectCurrent.FileCurrent.ViolationCurrent;
                }
                set { }
            }

            public Report()
            {
                Solutions = new List<Solution>();
            }

            public Solution CreateSolution() {
                Solution solution = new Solution();
                SolutionCurrent = solution;
                Solutions.Add(solution);
                return solution;
            }

            public File CreateFile()
            {
                return SolutionCurrent.ProjectCurrent.CreateFile();
            }

            public Violation CreateViolation()
            {
                return SolutionCurrent.ProjectCurrent.FileCurrent.CreateViolation();
            }

            public void NextFile()
            {
                ProjectCurrent.NextFile();
            }

            public void NextProject()
            {
                SolutionCurrent.NextProject();
            }

            public bool AddViolation(Violation violation)
            {
                if (!mapViolation.ContainsKey(violation.FullMessage))
                {
                    mapViolation[violation.FullMessage] = violation;
                    SolutionCurrent.ProjectCurrent.FileCurrent.Violations.Add(violation);
                    return true;
                }
                else 
                {
                    return false;
                }
            }
        }

    }
}
