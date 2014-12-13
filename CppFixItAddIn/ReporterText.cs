using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CppFixItAddIn
{
    namespace Reporting
    {
        class ReporterText
        {
            static public string CreateDisplay(Solution solution)
            {
                StringWriter stream = new StringWriter();
                Display(solution, stream);
                return stream.ToString();
            }

            static public string CreateDisplay(Project project)
            {
                StringWriter stream = new StringWriter();
                Display(project, stream);
                return stream.ToString();
            }

            static public void Display(Solution solution, StringWriter stream)
            {
                if (solution.Projects.Count == 0)
                {
                    return;
                }

                String dashLine = String.Empty.PadLeft(200, '-');
                stream.WriteLine(dashLine);
                stream.WriteLine(dashLine);
                stream.WriteLine(dashLine);
                stream.WriteLine("Solution name " + solution.Name);
                stream.WriteLine("Number of projects " + solution.Projects.Count);
                foreach(Project project in solution.Projects)
                {
                    Display(project, stream);
                }

                stream.WriteLine(solution.NumberOfProject + " projects");
                stream.WriteLine(solution.NumberOfFiles + " files");
                stream.WriteLine(solution.ViolationCount + " violations");
            }

            static public void Display(Project project, StringWriter stream)
            {
                if (project.ViolationCount == 0)
                {
                    return;
                }

                String margin = String.Empty.PadLeft(2, ' ');
                String dashLine = String.Empty.PadLeft(200, '-');
                stream.WriteLine(margin + dashLine);
                stream.WriteLine(margin + dashLine);
                stream.WriteLine(margin + "Project name " + project.Name);
                stream.WriteLine(margin + "Number of files " + project.Files.Count);
                foreach (File file in project.Files)
                {
                    Display(file, stream);
                }
            }

            static public void Display(File file, StringWriter stream)
            {
                if (file.Violations.Count == 0)
                {
                    return;
                }

                String margin = String.Empty.PadLeft(4, ' ');
                String dashLine = String.Empty.PadLeft(200, '-');
                stream.WriteLine(margin + dashLine);
                stream.WriteLine(margin + "File name " + file.Name + " @ " + file.FullPath);
                stream.WriteLine(margin + "Number of warnings: " + file.Violations.Count);
                foreach (Violation violation in file.Violations)
                {
                    Display(violation, stream);
                }

            }

            static public void Display(Violation violation, StringWriter stream)
            {
                String margin = String.Empty.PadLeft(6, ' ');
                stream.WriteLine(margin + "Violation");
                stream.WriteLine(violation.Message);
            }
        }
    }
}
