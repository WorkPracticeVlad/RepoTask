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
        Logger logger = LogManager.GetCurrentClassLogger();
        IContainer container = Container.For<LibRegistry>();
        Crawling crawler;
        SiteTreeBuilder treeBuilder;
        const string PATHFORTREEOUT = @"C: \Users\vorlov\Desktop\Tree\SiteTreeOut.txt";
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
                while (IsFileLocked(e.FullPath))
                {
                    Thread.Sleep(500);
                }
                var command = System.IO.File.ReadAllText(e.FullPath);
                string[] splitCommand = command.Split();
                crawler = container.GetInstance<Crawling>();
                if (splitCommand.Length == 2)
                {
                    watcherForCrawl.EnableRaisingEvents = true;
                    logger.Trace("Start crawl "+splitCommand[0]+" in "+splitCommand[1]+" threads");
                    Task.Run(() => crawler.StartCrawl(splitCommand[0], Int32.Parse(splitCommand[1])));
                    logger.Trace("Start crawl Task job done");
                }
                else
                {
                    watcherForCrawl.EnableRaisingEvents = true;      
                    Task.Run(() => crawler.Cancel());
                    logger.Trace("Cancel crawling");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message + Environment.NewLine + ex.InnerException);
            }         
        }
        void OnChangedTree(object source, FileSystemEventArgs e)
        {
            try
            {
                watcherForTree.EnableRaisingEvents = false;
                while (IsFileLocked(e.FullPath))
                {
                    Thread.Sleep(500);
                }
                var fullUrl = System.IO.File.ReadAllText(e.FullPath);
                var fullHost = m.HostFullAdr(fullUrl);
                treeBuilder = container.GetInstance<SiteTreeBuilder>();
                treeBuilder.WriteTree(PATHFORTREEOUT, fullHost);
                logger.Trace("Build tree for "+fullUrl);
                watcherForTree.EnableRaisingEvents = true;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message + Environment.NewLine + ex.InnerException);
            }        
        }
        bool IsFileLocked(string path)
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
                watcher.Changed += handler;
                watcher.EnableRaisingEvents = true;           
        }
        public void Dispose()
        {
            treeBuilder.Dispose();
            crawler.Dispose();
        }
    }
}
