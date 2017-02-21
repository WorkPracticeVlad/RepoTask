using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
namespace ClassLibrary.Data
{
   public class Repository : IRepository
    {
        private FistTaskEntities _ctx;
        public Repository()
        {
            _ctx = new FistTaskEntities();
        }
        public void AddPageOrUpdate(IEnumerable<PageUrls> items)
        {
            //opt
            foreach (var item in items)
            {
                var itemDB = _ctx.PageUrls.Where(p => p.Url == item.Url)
                    .Include(c => c.CssLinks)
                    .Include(i => i.ImgSources)
                    .Include(u => u.InternalUrls)
                    .Include(e => e.ExternalUrls)
                    .SingleOrDefault();
                if (itemDB == null)
                {
                    _ctx.PageUrls.Add(item);
                }
                else
                {
                    item.Id = itemDB.Id;
                    _ctx.Entry(itemDB).CurrentValues.SetValues(item);
                    foreach (var itemChild in itemDB.CssLinks.ToList())
                    {
                        if (!item.CssLinks.Any(c => c.Css == itemChild.Css && item.Url == itemChild.PageUrls.Url))
                            _ctx.CssLinks.Remove(itemChild);
                    }
                    foreach (var itemChild in itemDB.ImgSources.ToList())
                    {
                        if (!item.ImgSources.Any(c => c.Scr == itemChild.Scr && item.Url == itemChild.PageUrls.Url))
                            _ctx.ImgSources.Remove(itemChild);
                    }
                    foreach (var itemChild in itemDB.InternalUrls.ToList())
                    {
                        if (!item.InternalUrls.Any(c => c.Url == c.Url && item.Url == itemChild.PageUrls.Url))
                            _ctx.InternalUrls.Remove(itemChild);
                    }
                    foreach (var itemChild in itemDB.ExternalUrls.ToList())
                    {
                        if (!item.ExternalUrls.Any(c => c.Url == c.Url && item.Url == itemChild.PageUrls.Url))
                            _ctx.ExternalUrls.Remove(itemChild);
                    }
                    foreach (var child in item.CssLinks)
                    {
                        var existingChild = itemDB.CssLinks
                            .Where(c => c.Css == child.Css && c.PageUrls.Url == item.Url)
                            .FirstOrDefault();
                        if (existingChild == null)
                        {
                            var newChild = new CssLinks { Css = child.Css };
                            itemDB.CssLinks.Add(newChild);
                        }
                    }
                    foreach (var child in item.ImgSources)
                    {
                        var existingChild = itemDB.ImgSources
                            .Where(c => c.Scr == child.Scr && c.PageUrls.Url == item.Url)
                            .FirstOrDefault();
                        if (existingChild == null)
                        {
                            var newChild = new ImgSources { Scr = child.Scr };
                            itemDB.ImgSources.Add(newChild);
                        }
                    }
                    foreach (var child in item.InternalUrls)
                    {
                        var existingChild = itemDB.InternalUrls
                            .Where(c => c.Url == child.Url && c.PageUrls.Url == item.Url)
                            .FirstOrDefault();
                        if (existingChild == null)
                        {
                            var newChild = new InternalUrls { Url = child.Url };
                            itemDB.InternalUrls.Add(newChild);
                        }
                    }
                    foreach (var child in item.ExternalUrls)
                    {
                        var existingChild = itemDB.ExternalUrls
                            .Where(c => c.Url == child.Url && c.PageUrls.Url == item.Url)
                            .FirstOrDefault();
                        if (existingChild == null)
                        {
                            var newChild = new ExternalUrls { Url = child.Url };
                            itemDB.ExternalUrls.Add(newChild);
                        }
                    }
                }
            }
            _ctx.SaveChanges();
        }
        public void Dispose()
        {
            _ctx.Dispose();
        }
        public List<string> GetUrlsForHost(string hostUrl)
        {
            return _ctx.PageUrls.Where(r => r.Url.StartsWith(hostUrl)).Select(i => i.Url).ToList();
        }
        public int? GetHostIdIfExist(string hostName)
        {
            var host = _ctx.Hosts.SingleOrDefault(h => h.Host == hostName);
            if (host != null)
                return host.Id;
            return null;
        }
        public void AddHost(string hostName)
        {
            var host = new Hosts();
            host.Host = hostName;
            _ctx.Hosts.Add(host);
            _ctx.SaveChanges();
        }
    }
}
