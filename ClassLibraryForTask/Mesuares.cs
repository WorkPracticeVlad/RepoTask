using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibraryForTask
{
    class Mesuares
    {
        Stopwatch _timeSpan = new Stopwatch();
        public PageUrl TakeMesuares(string fullUrl)
        {
            var res = new PageUrl();
            var webGet = new HtmlWeb();
            try
            {
                _timeSpan.Start();
                var document = webGet.Load(fullUrl);
                _timeSpan.Stop();
                var loadTime = _timeSpan.Elapsed.Milliseconds;
                try
                {
                    string html = document.DocumentNode.OuterHtml;
                    //Console.Write(html);
                    byte[] toBytes = Encoding.ASCII.GetBytes(html);
                    int x = toBytes.Length;
                    //float mb = (toBytes.Length / 1024f) / 1024f;
                    int htmlSize = System.Text.ASCIIEncoding.Unicode.GetByteCount(html);
                    var cssLinksHrefs = GetcssLinksHrefs(document);
                    var imgSources = GetImgSources(document);
                    var urlsInternal = GetUrlsInternal(GetHrefs(document, fullUrl), fullUrl);
                    var urlsExternal = GetUrlsExtrnal(GetHrefs(document, fullUrl), fullUrl);
                    res.Url = fullUrl;
                    res.InternalUrl = urlsInternal;
                    res.ExternalUrl = urlsExternal;
                    res.LoadTime = loadTime;
                    res.HtmlSize = htmlSize;
                    res.CssLink = cssLinksHrefs;
                    res.ImgSource = imgSources;
                    return res;
                }
                catch (Exception)
                {
                    res.Url = fullUrl;
                    res.InternalUrl = null;
                    res.ExternalUrl = null;
                    res.LoadTime = loadTime;
                    res.HtmlSize = 0;
                    res.CssLink = null;
                    res.ImgSource = null;
                    return res;
                }
            }
            catch (Exception)
            {
                res.Url = fullUrl;
                return res;
            }
        }
        List<CssLink> GetcssLinksHrefs(HtmlDocument document)
        {
            var linkTags = document.DocumentNode.Descendants("link").Select(e => new { rel = e.GetAttributeValue("rel", null), href = e.GetAttributeValue("href", null) });
            List<CssLink> linksSource = new List<CssLink>();
            foreach (var link in linkTags)
            {
                if (link.rel == "stylesheet")
                    linksSource.Add(new CssLink { Css = link.href });
            }
            return linksSource;
        }
        List<ImgSource> GetImgSources(HtmlDocument document)
        {
            var srcs = document.DocumentNode.Descendants("img")
                                            .Select(e => e.GetAttributeValue("src", null))
                                            .Where(s => !String.IsNullOrEmpty(s));
            List<ImgSource> linksSource = new List<ImgSource>();
            foreach (var scr in srcs)
            {
                linksSource.Add(new ImgSource { Scr = scr });
            }
            return linksSource.ToList();
        }
        List<string> GetHrefs(HtmlDocument document, string url)
        {
            var hrefs = document.DocumentNode.Descendants("a")
                                           .Select(e => e.GetAttributeValue("href", null))
                                           .Where(u => !String.IsNullOrEmpty(u)).ToList();
            hrefs = UrlsClean(hrefs, url);
            return hrefs.Distinct().ToList();
        }
        List<string> UrlsClean(List<string> hrefs, string url)
        {
            Uri myUri = new Uri(url);
            var protocol = myUri.Scheme;
            string host = myUri.Host;
            var temp = protocol + "://" + host;
            List<string> urlCleanList = new List<string>();
            foreach (var href in hrefs)
            {
                if (href != null && href.Contains("/") && href != "")
                {
                    urlCleanList.Add(href);
                }
            }
            return urlCleanList;
        }
        List<InternalUrl> GetUrlsInternal(List<string> hrefs, string url)
        {
            Uri myUri = new Uri(url);
            var protocol = myUri.Scheme;
            string host = myUri.Host;
            var temp = protocol + "://" + host;
            List<InternalUrl> urlInternalList = new List<InternalUrl>();
            foreach (var href in hrefs)
            {
                if (href.StartsWith(temp))
                    urlInternalList.Add(new InternalUrl { Url = href });
                if (href.StartsWith("//" + host))
                    urlInternalList.Add(new InternalUrl { Url = protocol + ":" + href });
                if (href.StartsWith("/") && !href.StartsWith("//"))
                    urlInternalList.Add(new InternalUrl { Url = temp + href });
            }
            return urlInternalList.Distinct().ToList();
        }
        List<ExternalUrl> GetUrlsExtrnal(List<string> hrefs, string url)
        {
            List<ExternalUrl> urlExternallList = new List<ExternalUrl>();
            Uri myUri = new Uri(url);
            var protocol = myUri.Scheme;
            string host = myUri.Host;
            var temp = protocol + "://" + host;
            foreach (var href in hrefs)
            {
                if ((href.StartsWith("http://") || href.StartsWith("https://")) && !href.StartsWith(temp))
                    urlExternallList.Add(new ExternalUrl { Url = href });
                if (href.StartsWith("//") && !href.StartsWith("//" + host))
                    urlExternallList.Add(new ExternalUrl { Url = protocol + ":" + href });
            }
            return urlExternallList.ToList();
        }
    }
}
