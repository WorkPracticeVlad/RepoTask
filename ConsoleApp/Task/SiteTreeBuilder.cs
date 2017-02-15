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
        void WriteTree(string path, string url)
        {
            List<string> urls = _repo.GetUrlsForHost(url);
            if (!File.Exists(path))
            {
                string createText = "Hello and Welcome" + Environment.NewLine + BuildSiteTreeString(url, urls);
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
            //var wt = (FileSystemWatcher)source;
            watcher.EnableRaisingEvents = false; 
            while (IsFileLocked(@"C:\Users\vorlov\Desktop\BuildTree\Demand.txt"))
            {
                Thread.Sleep(500);
            }
            var fullUrl= System.IO.File.ReadAllText(@"C:\Users\vorlov\Desktop\BuildTree\Demand.txt");
            var myUri = new Uri(fullUrl);
            var protocol = myUri.Scheme;
            var host = myUri.Host;
            var fullHost = protocol + "://" + host;
            WriteTree("C:\\Users\\vorlov\\Desktop\\Output\\Tree.txt", fullHost);
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
