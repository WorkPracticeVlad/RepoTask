using ConsoleApp.Data;
using ConsoleApp.Task;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var rep = new Repository(new FistTaskEntities()))
            {
                var tb = new SiteTreeBuilder(rep);
                tb.WatchFolder("C:\\Users\\vorlov\\Desktop\\BuildTree");
                var pr = new Crawling(rep);
                pr.StartCrawl("https://www.everafterguide.com/coaster-furniture-dark-brown-tv-console-with-floating-top-shelf-4c2031cbbdb6f70c.html", 4);
            }
        }
    }
}
