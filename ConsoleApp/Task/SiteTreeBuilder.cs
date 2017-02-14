using ConsoleApp.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp.Task
{
    class SiteTreeBuilder
    {
        public SiteTreeBuilder(IRepository repo)
        {
            _repo = repo;
        }
        FileSystemWatcher watcher = new FileSystemWatcher();
        private IRepository _repo;

        string BuildSiteTreeString(string url, List<string> urls)
        {

            var builder = new StringBuilder();
            urls.Sort();
            foreach (var i in urls)
            {
                    builder.AppendLine(i);
            }
            return builder.ToString();
        }
        void WriteTree(string path, string url )
        {
            List<string> urls = _repo.GetUrlsForHost(url);
            if (!File.Exists(path))
            {
                string createText = "Hello and Welcome" + Environment.NewLine + BuildSiteTreeString(url,  urls);
                File.WriteAllText(path, createText);
            }
            string appendText = "This is extra text" + Environment.NewLine + BuildSiteTreeString(url, urls);
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
            var wt = (FileSystemWatcher)source;
            string fullUrl="";
            watcher.EnableRaisingEvents = false;
            try
            {   
                using (StreamReader sr = new StreamReader(wt.Path+"\\Demand.txt"))
                {                   
                    fullUrl = sr.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(ex.Message);
            }
            var myUri = new Uri(fullUrl);
            var protocol = myUri.Scheme;
            var host = myUri.Host;
            var fullHost = protocol + "://" + host;
            WriteTree("C:\\Users\\vorlov\\Desktop\\Output\\Tree.txt", fullHost);
            watcher.EnableRaisingEvents = true;
        }
    }
}
