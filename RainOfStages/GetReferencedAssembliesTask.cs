using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RainOfStages.Plugin
{
    public class GetReferencedAssembliesTask : AppDomainIsolatedTask
    {
        public string AssemblyPath { get; set; }
        public override bool Execute()
        {
            return true;
        }
    }
}
