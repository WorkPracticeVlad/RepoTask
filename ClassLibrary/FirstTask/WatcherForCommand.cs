using ClassLibrary.Data;
using NLog;
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
        private IRepository _repo;
        public WatcherForCommand(IRepository repo)
        {
            _repo = repo;
        }
        public void WatchFolderStartOrCancel(string path)
        {
            WatchTxt(path, watcherForCrawl, new FileSystemEventHandler(OnChangedStartOrCancel));           
        }
        public void WatchFolderTree(string path)
        {
            WatchTxt(path, watcherForTree, new FileSystemEventHandler(OnChangedTree));          
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
                var pr = new Crawling(_repo);
                if (splitCommand.Length == 2)
                {
                    watcherForCrawl.EnableRaisingEvents = true;
                    lgr.Trace("Start crawl "+splitCommand[0]+" in "+splitCommand[1]+" threads");
                    System.Threading.Tasks.Task.Run(() => pr.StartCrawl(splitCommand[0], Int32.Parse(splitCommand[1])));
                }
                else
                {
                    watcherForCrawl.EnableRaisingEvents = true;
                    lgr.Trace("Cancel crawling");
                    System.Threading.Tasks.Task.Run(() => pr.Cancel());
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
                var myUri = new Uri(fullUrl);
                var protocol = myUri.Scheme;
                var host = myUri.Host;
                var fullHost = protocol + "://" + host;
                var tb = new SiteTreeBuilder(_repo);
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
    }
}
