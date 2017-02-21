using ClassLibrary.Data;
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
    public class MyService
    {
        IContainer con = Container.For<LibRegistry>();
        WatcherForCommand wt;
        Logger lgr = LogManager.GetCurrentClassLogger();
        public void OnStart()
        {
            wt = con.GetInstance<WatcherForCommand>();
            try
            {
                wt.StartWatch(@"C:\Users\vorlov\Desktop\FistTaskStart", @"C:\Users\vorlov\Desktop\BuildTree");
            }
            catch (Exception ex)
            {
                lgr.Error(ex.Message + Environment.NewLine + ex.InnerException);
            }
            Console.ReadKey();
        }
        public void OnStop()
        {
            wt.Dispose();
        }
    }
}
