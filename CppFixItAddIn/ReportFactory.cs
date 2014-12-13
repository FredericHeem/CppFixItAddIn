using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.VCProject;
using Microsoft.VisualStudio.VCProjectEngine;
using CppFixItAddIn.Reporting;

namespace CppFixItAddIn
{
    namespace Reporting
    {
        class ReportFactory
        {
            static public Report CreateReportSingleFile(EnvDTE.Solution dteSolution, VCFile vcFile)
            {
                Report report = new Report();
                //Solution
                Reporting.Solution solution = AddSolution(report, dteSolution);

                //Project
                Reporting.Project project = solution.CreateProject();
                EnvDTE.Project dteProject = DTE2Utils.GetProject(vcFile);
                if (dteProject != null)
                {
                    project.Name = dteProject.Name;
                }

                //File
                Reporting.File file = project.CreateFile();
                file.Name = vcFile.Name;
                file.FullPath = vcFile.FullPath;

                return report;

            }

            static public Report CreateReportForVCProject(EnvDTE.Solution dteSolution, VCProject vcProject)
            {
                Report report = new Report();
                //Solution
                Reporting.Solution solution = AddSolution(report, dteSolution);

                AddProject(solution, vcProject);

                return report;
            }

            static public Report CreateReportFromVsSolution(EnvDTE.Solution dteSolution, Queue<VCProject> vcProjectQueue)
            {
                Report report = new Report();
                //Solution
                Reporting.Solution solution = AddSolution(report, dteSolution);

                foreach (VCProject vcProject in vcProjectQueue)
                {
                    AddProject(solution, vcProject);
                }

                return report;
            }


            static private Reporting.Solution AddSolution(Report report, EnvDTE.Solution dteSolution)
            {
                Reporting.Solution solution = report.CreateSolution();
                solution.Name = dteSolution.FullName;
                return solution;
            }

            static private void AddProject(Reporting.Solution solution, VCProject vcProject)
            {
                //Project
                Queue<VCFile> vcFileQueue = DTE2Utils.CreateFileQueue(vcProject);
                Reporting.Project project = solution.CreateProject();

                EnvDTE.Project dteProject = vcProject.Object as EnvDTE.Project;
                if (dteProject != null)
                {
                    project.Name = dteProject.Name;
                }

                foreach (VCFile vcFile in vcFileQueue)
                {
                    Reporting.File file = project.CreateFile();
                    file.Name = vcFile.Name;
                    file.FullPath = vcFile.FullPath;
                }
            }
        }
    }
}
