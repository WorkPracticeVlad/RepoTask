using ClassLibrary.FirstTask;
using NLog;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            var con = Container.For<AppRegistry>();
            var wt = con.GetInstance<WatcherForCommand>();
            Logger lgr = LogManager.GetCurrentClassLogger();
            try
            {
                wt.WatchFolderTree(@"C:\Users\vorlov\Desktop\BuildTree");
                wt.WatchFolderStartOrCancel(@"C:\Users\vorlov\Desktop\FistTaskStart");
            }
            catch (Exception ex)
            {

                lgr.Error(ex.Message + Environment.NewLine + ex.InnerException);
            }
           
            Console.ReadKey();
        }
    }
}
