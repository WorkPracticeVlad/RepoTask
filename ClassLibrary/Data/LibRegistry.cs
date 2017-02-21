using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.Data
{
   public class LibRegistry : Registry
    {
        public LibRegistry()
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
