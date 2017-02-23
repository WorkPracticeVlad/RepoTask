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
            var urls = items.Select(r => r.Url).ToArray();
            var itemsDB = _ctx.PageUrls.Where(item => urls.Contains(item.Url))
                    .Include(c => c.CssLinks)
                    .Include(i => i.ImgSources)
                    .Include(u => u.InternalUrls)
                    .Include(e => e.ExternalUrls)
                    .ToArray();
            foreach (var item in items)
            {
                var itemDB = itemsDB.Where(p => p.Url == item.Url)
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
                        if (!item.CssLinks.Any(c => c.Css == itemChild.Css))
                            _ctx.CssLinks.Remove(itemChild);
                    }
                    foreach (var itemChild in itemDB.ImgSources.ToList())
                    {
                        if (!item.ImgSources.Any(c => c.Scr == itemChild.Scr ))
                            _ctx.ImgSources.Remove(itemChild);
                    }
                    foreach (var itemChild in itemDB.InternalUrls.ToList())
                    {
                        if (!item.InternalUrls.Any(c => c.Url == c.Url ))
                            _ctx.InternalUrls.Remove(itemChild);
                    }
                    foreach (var itemChild in itemDB.ExternalUrls.ToList())
                    {
                        if (!item.ExternalUrls.Any(c => c.Url == c.Url ))
                            _ctx.ExternalUrls.Remove(itemChild);
                    }
                    foreach (var child in item.CssLinks)
                    {
                        if(!itemDB.CssLinks.Any(c => c.Css == child.Css))
                            itemDB.CssLinks.Add(new CssLinks { Css = child.Css });                       
                    }
                    foreach (var child in item.ImgSources)
                    {
                        if (!itemDB.ImgSources.Any(i=>i.Scr==child.Scr))
                            itemDB.ImgSources.Add(new ImgSources { Scr = child.Scr });                       
                    }
                    foreach (var child in item.InternalUrls)
                    {
                        if (!itemDB.InternalUrls.Any(i => i.Url == child.Url))
                            itemDB.InternalUrls.Add(new InternalUrls { Url = child.Url });                     
                    }
                    foreach (var child in item.ExternalUrls)
                    {
                        if (!itemDB.ExternalUrls.Any(i => i.Url == child.Url))
                            itemDB.ExternalUrls.Add(new ExternalUrls { Url = child.Url });                       
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
