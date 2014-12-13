using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE;
using Extensibility;

namespace CppFixItAddIn
{
    /// <summary>
    /// Visual Studio 2010 Add-in for c++ static analysis
    /// </summary>
    public class Connect2010Debug : Connect
    {
        /// <summary>
        /// 
        /// </summary>
        public Connect2010Debug()
            : base("CppFixItDebug2010")
        {
            ConnectName = "Connect2010Debug";
        }
    }
}
