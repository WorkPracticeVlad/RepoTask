using ClassLibrary.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.FirstTask
{
    class SiteTreeBuilder
    {
        public SiteTreeBuilder(IRepository repo)
        {
            _repo = repo;
        }
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
        internal void WriteTree(string path, string url)
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
            //_repo.Dispose();
        }
        public void Dispose()
        {
            _repo.Dispose();
        }
    }
}
