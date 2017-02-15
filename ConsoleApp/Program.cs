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
            var wt = new WatcherForCommand();
            wt.WatchFolder("C:\\Users\\vorlov\\Desktop\\FistTaskStart");
            Console.ReadKey();
        }
    }
}
