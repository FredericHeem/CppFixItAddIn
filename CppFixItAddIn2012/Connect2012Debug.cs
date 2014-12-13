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
    public class Connect2012Debug : Connect
    {
        /// <summary>
        /// 
        /// </summary>
        public Connect2012Debug()
            : base("CppFixItDebug2012")
        {
            ConnectName = "Connect2012Debug";
        }
    }
}
