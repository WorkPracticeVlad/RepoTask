using ClassLibraryForTask;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleAppForTask
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = Container.For<ConRegistry>();
            using (Repository rep=new Repository(new FistTaskEntities()))
            {
                Crawling cr = new Crawling(rep);
                cr.StartCrawl("http://www.sql-tutorial.ru/", 2);
            }  
        }
    }
}
