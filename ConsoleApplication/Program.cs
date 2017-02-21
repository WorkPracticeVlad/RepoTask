using ClassLibrary.Data;
using ClassLibrary.FirstTask;
using NLog;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace ConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            //HostFactory.Run(configure =>
            //{
            //    configure.Service<MyService>(service =>
            //    {
            //        service.ConstructUsing(s => new MyService());
            //        service.WhenStarted(s => s.OnStart());
            //        service.WhenStopped(s => s.OnStop());
            //    });
            //    configure.RunAsLocalSystem();
            //    configure.SetServiceName("MyWindowServiceWithTopshelf");
            //    configure.SetDisplayName("MyWindowServiceWithTopshelf");
            //    configure.SetDescription("MyWindowServiceWithTopshelf");
            //});
            WorkConsApp();
        }
        static void WorkConsApp()
        {
            var con = Container.For<LibRegistry>();
            var wt = con.GetInstance<WatcherForCommand>();
            Logger lgr = LogManager.GetCurrentClassLogger();
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
    }
}
