using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibraryForTask
{
    public class SiteTreeBuilder
    {
        FileSystemWatcher watcher = new FileSystemWatcher();
         string BuildSiteTreeString(string fullUrl, List<string> urls)
        {
            var myUri = new Uri(fullUrl);
            var protocol = myUri.Scheme;
            var host = myUri.Host;
            var builder = new StringBuilder();
            var fullHost = protocol + "://" + host;
            urls.Sort();
            foreach (var i in urls)
            {
                if (i.StartsWith(fullHost))
                    builder.AppendLine(i);
            }
            return builder.ToString();
        }
         void WriteTree(string path)
        {
            if (!File.Exists(path))
            {
                string createText = "Hello and Welcome" + Environment.NewLine;// + BuildSiteTreeString(string fullUrl, List < string > urls);
                File.WriteAllText(path, createText);
            }       
            string appendText = "This is extra text" + Environment.NewLine;// + BuildSiteTreeString(string fullUrl, List < string > urls);
            File.AppendAllText(path, appendText);
            string readText = File.ReadAllText(path);
            Console.WriteLine(readText);
        }
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
            WriteTree("C:\\Users\\vorlov\\Desktop\\Output\\Tree.txt");
            watcher.EnableRaisingEvents = true;
        }
    }
}
