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
        IContainer container = Container.For<LibRegistry>();
        WatcherForCommand watcher;
        Logger logger = LogManager.GetCurrentClassLogger();
        public void OnStart()
        {
            watcher = container.GetInstance<WatcherForCommand>();
            try
            {
                watcher.StartWatch(@"C:\Users\vorlov\Desktop\FistTaskStart", @"C:\Users\vorlov\Desktop\BuildTree");
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message + Environment.NewLine + ex.InnerException);
            }

        }
        
        public void OnStop()
        {
            try
            {
                watcher.Dispose();
            }
            catch (Exception ex)
            {

                logger.Error(ex.Message + Environment.NewLine + ex.InnerException);
            }
            
        }
    }
}
