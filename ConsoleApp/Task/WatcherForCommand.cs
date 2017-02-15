using ConsoleApp.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp.Task
{
    class WatcherForCommand
    {
        FileSystemWatcher watcher = new FileSystemWatcher();
        public void WatchFolder(string path)
        {
                
                watcher.Path = path;
                watcher.NotifyFilter = NotifyFilters.LastWrite;
                watcher.Filter = "*.txt";
                watcher.Changed += new FileSystemEventHandler(OnChanged);
                watcher.EnableRaisingEvents = true;
        }
        void OnChanged(object source, FileSystemEventArgs e)
        {
            //var wt = (FileSystemWatcher)source;
            watcher.EnableRaisingEvents = false;
            while (IsFileLocked(@"C:\Users\vorlov\Desktop\FistTaskStart\Command.txt"))
            {
                Thread.Sleep(500);
            }
            var command = System.IO.File.ReadAllText(@"C:\Users\vorlov\Desktop\FistTaskStart\Command.txt");
            string[] splitCommand = command.Split();
            using (var rep = new Repository(new FistTaskEntities()))
            {

                //var tb = new SiteTreeBuilder(rep);
                //tb.WatchFolder("C:\\Users\\vorlov\\Desktop\\BuildTree");
                var pr = new Crawling(rep);
                pr.StartCrawl(splitCommand[0], Int32.Parse(splitCommand[1]));
            }
            watcher.EnableRaisingEvents = true;
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
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }

    }
}
