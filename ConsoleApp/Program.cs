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
            //var wt = new WatcherForCommand();
            //wt.WatchFolder("C:\\Users\\vorlov\\Desktop\\FistTaskStart");
            //Console.ReadKey();
            using (var rep = new Repository(new FistTaskEntities()))
            {
                //var tb = new SiteTreeBuilder(rep);
                //tb.WatchFolder("C:\\Users\\vorlov\\Desktop\\BuildTree");
                var pr = new Crawling(rep);
                pr.StartCrawl("https://learn.javascript.ru/", 20);
            }
        }
    }
}
