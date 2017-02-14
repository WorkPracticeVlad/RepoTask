using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibraryForTask
{
    public class Repository : IRepository
    {
        FistTaskEntities _cntx;
        public Repository(FistTaskEntities cntx)
        {
            _cntx = cntx;
        }
     

        public void Add(PageUrl pg)
        {
            lock (_cntx)
            {
                _cntx.PageUrl.Add(pg);
                //_cntx.SaveChanges();
            }      
        }
        public void Dispose()
        {
            _cntx.Dispose();
        }

        public List<string> GetExternalUrl(string url)
        {
            lock (_cntx)
            {
                return _cntx.PageUrl.Single(u => u.Url == url).ExternalUrl.Select(e => e.Url).ToList();
            }
        }

        public List<string> GetInternalUrl(string url)
        {
            lock (_cntx)
            {
                return _cntx.PageUrl.Single(u => u.Url == url).InternalUrl.Select(e => e.Url).ToList();
            }
        }

        public List<string> GetUrlsAll()
        {
            return _cntx.PageUrl.Select(u=>u.Url).ToList();
        }

        public List<string> GetUrlsSite(string url)
        {
            return _cntx.PageUrl.Where(i=>i.Url.StartsWith(url)).Select(u => u.Url).ToList();
        }
    }
}
