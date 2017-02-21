using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.Data
{
   public interface IRepository : IDisposable
    {
        void AddPageOrUpdate(IEnumerable<PageUrls> items);
        int? GetHostIdIfExist(string hostName);
        void AddHost(string hostName);
        List<string> GetUrlsForHost(string hostUrl);
    }
}
