using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp.Data
{
    class Repository : IRepository
    {
        private FistTaskEntities _ctx;
        public Repository(FistTaskEntities ctx)
        {
            _ctx = ctx;
        }
        public void Add(IEnumerable<PageUrls> items)
        {
            _ctx.PageUrls.AddRange(items);
            _ctx.SaveChanges();
            //foreach (var item in items)
            //{
            //    _ctx.PageUrls.Add(item);
            //    _ctx.SaveChanges();
            //}          
        }
        public void Dispose()
        {
            _ctx.Dispose();
        }

        public List<string> GetUrlsForHost(string hostUrl)
        {       
            return _ctx.PageUrls.Where(r=>r.Url.StartsWith(hostUrl)).Select(i=>i.Url).ToList();
        }
    }
}
