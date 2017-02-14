using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibraryForTask
{
    public class WatchForCommand
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
            watcher.EnableRaisingEvents = false;
            Console.WriteLine(e.ChangeType +" "+ e.Name);
            watcher.EnableRaisingEvents = true;
        }
    }
}
