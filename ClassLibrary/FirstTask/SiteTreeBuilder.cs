using ClassLibrary.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.FirstTask
{
   public class SiteTreeBuilder
    {
        public SiteTreeBuilder(IRepository repo)
        {
            _repo = repo;
        }
        private IRepository _repo;
        string BuildTreeString( List<string> urls)
        {
            if (urls.Count == 0)
                return "no match in DB";
            var builder = new StringBuilder();
            urls.Sort();
            foreach (var i in urls)
            {
                builder.AppendLine(i);
            }
            return builder.ToString();
        }
        public string WriteTree(string path, string url)
        {
            var urls = new List<string>();
            urls = _repo.GetUrlsForHost(url);
            if (!File.Exists(path))
            {
                string createText = "Hello and Welcome " + Environment.NewLine;
                File.WriteAllText(path, createText);
            }
            string appendText = "This is tree" + Environment.NewLine + BuildTreeString(urls)+ Environment.NewLine;
            File.AppendAllText(path, appendText);
            //string readText = File.ReadAllText(path);
            return appendText;
        }
        public void Dispose()
        {
            _repo.Dispose();
        }
    }
}
