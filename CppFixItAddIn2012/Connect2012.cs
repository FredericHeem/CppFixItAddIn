using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE;
using Extensibility;

namespace CppFixItAddIn
{
    /// <summary>
    /// Visual Studio 2012 Add-in for c++ static analysis
    /// </summary>
    public class Connect2012 : Connect
    {
        /// <summary>
        /// 
        /// </summary>
        public Connect2012()
            : base("CppFixIt2012")
        {
            ConnectName = "Connect2012";
        }
    }
}
