using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp.Data
{
    interface IRepository : IDisposable
    {
        void Add(IEnumerable<PageUrls> items);
        List<string> GetUrlsForHost(string hostUrl);
    }
}
