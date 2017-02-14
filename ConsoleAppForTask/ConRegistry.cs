using ClassLibraryForTask;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleAppForTask
{
    class ConRegistry:Registry
    {
        public ConRegistry()
        {
            Scan(scan =>
            {
                scan.TheCallingAssembly();
                scan.WithDefaultConventions();
            });
            For<IRepository>().Use<Repository>();
        }
    }
}
