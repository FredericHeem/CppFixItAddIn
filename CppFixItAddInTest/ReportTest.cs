using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Text.RegularExpressions;

namespace CppFixItAddIn
{
     [TestFixture]
    class ReportTest
    {
         private Regex regExFileLineColumnMessage = new Regex(@"(\D.+)\((\d+),(\d+)\)\s:\s+(warning|error|fatal\serror):\s(.+)");
         [SetUp]
         public void Setup()
         {
         
         }

         [Test]
         public void TestWarning()
         {
             string lineWaring = "C:\\Users\\frederic\\Documents\\projects\\StateForge\\dev\\StateEditor\\src\\StateEditorMainWindow.cpp(75,21) :  warning: Value stored to 'pApp' during its initialization is never read";
             Match match = regExFileLineColumnMessage.Match(lineWaring);
             Assert.True(match.Success);
             Console.WriteLine("#groups " + match.Groups.Count);

             foreach (Group group in match.Groups)
             {
                 Console.WriteLine("The value '{0}' was found at index {1}, and is {2} characters long.",
                     group.Value, group.Index, group.Length);
             }  

             Assert.True(match.Groups.Count == 6);
         }

         [Test]
         public void TestError()
         {
             string lineWaring = "C:\\Users\\frederic\\Documents\\projects\\StateForge\\dev\\StateEditor\\src\\StateEditorMainWindow.cpp(75,21) :  error: Value stored to 'pApp' during its initialization is never read";
             Match match = regExFileLineColumnMessage.Match(lineWaring);
             Assert.True(match.Success);
             Console.WriteLine("#groups " + match.Groups.Count);

             Assert.True(match.Groups.Count == 6);
             Assert.AreEqual("C:\\Users\\frederic\\Documents\\projects\\StateForge\\dev\\StateEditor\\src\\StateEditorMainWindow.cpp", match.Groups[1].Value);
             Assert.AreEqual("75", match.Groups[2].Value);
             Assert.AreEqual("21", match.Groups[3].Value);
             Assert.AreEqual("error", match.Groups[4].Value);
             Assert.AreEqual("Value stored to 'pApp' during its initialization is never read", match.Groups[5].Value);
         }
         [Test]
         public void TestFatalError()
         {
             string lineWaring = "C:\\Users\\frederic\\Documents\\projects\\StateForge\\dev\\StateEditor\\src\\StateEditorMainWindow.cpp(75,21) :  fatal error: Value stored to 'pApp' during its initialization is never read";
             Match match = regExFileLineColumnMessage.Match(lineWaring);
             Assert.True(match.Success);
             Console.WriteLine("#groups " + match.Groups.Count);

             Assert.True(match.Groups.Count == 6);
             Assert.AreEqual("C:\\Users\\frederic\\Documents\\projects\\StateForge\\dev\\StateEditor\\src\\StateEditorMainWindow.cpp", match.Groups[1].Value);
             Assert.AreEqual("75", match.Groups[2].Value);
             Assert.AreEqual("21", match.Groups[3].Value);
             Assert.AreEqual("fatal error", match.Groups[4].Value);
             Assert.AreEqual("Value stored to 'pApp' during its initialization is never read", match.Groups[5].Value);
         }

        static void Main(string[] args)
        {
        }
    }
}
