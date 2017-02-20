using ClassLibrary.Data;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication
{
    class AppRegistry : Registry
    {
        public AppRegistry()
        {
            Scan(scan =>
            {
                scan.AssembliesFromApplicationBaseDirectory();
                scan.TheCallingAssembly();
                scan.WithDefaultConventions();
            });
            For<IRepository>().Use<Repository>();
        }
    }
}
