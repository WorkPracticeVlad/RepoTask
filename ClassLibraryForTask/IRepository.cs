using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibraryForTask
{
    public interface IRepository : IDisposable
    {
        void Add(PageUrl pg);
        List<string> GetUrlsAll();
        List<string> GetUrlsSite(string url);
        List<string> GetInternalUrl(string url);
        List<string> GetExternalUrl(string url);
    }
}
