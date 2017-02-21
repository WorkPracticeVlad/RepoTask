using ClassLibrary.Data;
using NLog;
using StructureMap;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClassLibrary.FirstTask
{
   public class WatcherForCommand
    {
        FileSystemWatcher watcherForCrawl = new FileSystemWatcher();
        FileSystemWatcher watcherForTree = new FileSystemWatcher();        
        Logger lgr = LogManager.GetCurrentClassLogger();
        IContainer con = Container.For<LibRegistry>();
        Crawling cr;
        SiteTreeBuilder tb;
        Measures m = new Measures();
        public void StartWatch(string startOrCancelPath, string treePath)
        {
            WatchTxt(treePath, watcherForTree, new FileSystemEventHandler(OnChangedTree));
            WatchTxt(startOrCancelPath, watcherForCrawl, new FileSystemEventHandler(OnChangedStartOrCancel));   
        }
        void OnChangedStartOrCancel(object source, FileSystemEventArgs e)
        {
            try
            {
                watcherForCrawl.EnableRaisingEvents = false;
                while (IsFileLocked(@"C:\Users\vorlov\Desktop\FistTaskStart\Command.txt"))
                {
                    Thread.Sleep(500);
                }
                var command = System.IO.File.ReadAllText(@"C:\Users\vorlov\Desktop\FistTaskStart\Command.txt");
                string[] splitCommand = command.Split();
                cr = con.GetInstance<Crawling>();
                if (splitCommand.Length == 2)
                {
                    watcherForCrawl.EnableRaisingEvents = true;
                    lgr.Trace("Start crawl "+splitCommand[0]+" in "+splitCommand[1]+" threads");
                    System.Threading.Tasks.Task.Run(() => cr.StartCrawl(splitCommand[0], Int32.Parse(splitCommand[1])));
                }
                else
                {
                    watcherForCrawl.EnableRaisingEvents = true;
                    lgr.Trace("Cancel crawling");
                    System.Threading.Tasks.Task.Run(() => cr.Cancel());
                }
            }
            catch (Exception ex)
            {
                lgr.Error(ex.Message + Environment.NewLine + ex.InnerException);
            }         
        }
        void OnChangedTree(object source, FileSystemEventArgs e)
        {
            try
            {
                watcherForTree.EnableRaisingEvents = false;
                while (IsFileLocked(@"C:\Users\vorlov\Desktop\BuildTree\Demand.txt"))
                {
                    Thread.Sleep(500);
                }
                var fullUrl = System.IO.File.ReadAllText(@"C:\Users\vorlov\Desktop\BuildTree\Demand.txt");
                var fullHost = m.HostFullAdr(fullUrl);
                tb = con.GetInstance<SiteTreeBuilder>();
                tb.WriteTree(@"C:\Users\vorlov\Desktop\Output\Tree.txt", fullHost);
                lgr.Trace("Build tree for "+fullUrl);
                watcherForTree.EnableRaisingEvents = true;
            }
            catch (Exception ex)
            {
                lgr.Error(ex.Message + Environment.NewLine + ex.InnerException);
            }        
        }
        private bool IsFileLocked(string path)
        {
            FileStream stream = null;
            try
            {
                stream = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
            return false;
        }
        void WatchTxt(string path,FileSystemWatcher watcher, FileSystemEventHandler handler)
        {
            watcher.Path = path;
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Filter = "*.txt";
            watcher.Changed +=handler;
            watcher.EnableRaisingEvents = true;
        }
        public void Dispose()
        {
            tb.Dispose();
            cr.Dispose();
        }
    }
}
