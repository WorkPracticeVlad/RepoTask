using ClassLibrary.Data;
using HtmlAgilityPack;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.FirstTask
{
    public class Measures
    {
        Stopwatch _timeSpan = new Stopwatch();
        Logger logger= LogManager.GetCurrentClassLogger();
        public PageUrls TakeMesuares(string fullUrl)
        {
            var res = new PageUrls();
            var webGet = new HtmlWeb();
            try
            {
                _timeSpan.Start();
                var document = webGet.Load(fullUrl);
                _timeSpan.Stop();
                var loadTime = _timeSpan.Elapsed.Milliseconds;
                string html = document.DocumentNode.OuterHtml;
                byte[] toBytes = Encoding.ASCII.GetBytes(html);
                int x = toBytes.Length;
                int htmlSize = System.Text.ASCIIEncoding.Unicode.GetByteCount(html);
                var cssLinksHrefs = GetCssLinksHrefs(document);
                var imgSources = GetImgSources(document);
                var urlsInternal = GetUrlsInternal(GetHrefs(document, fullUrl), fullUrl);
                var urlsExternal = GetUrlsExtrnal(GetHrefs(document, fullUrl), fullUrl);
                res.Url = fullUrl;
                res.InternalUrls = urlsInternal;
                res.ExternalUrls = urlsExternal;
                res.LoadTime = loadTime;
                res.HtmlSize = htmlSize;
                res.CssLinks = cssLinksHrefs;
                res.ImgSources = imgSources;
                return res;
            }
            catch (Exception e)
            {
                logger.Error(fullUrl+e.Message+e.InnerException);
                res.Url = fullUrl;
                return res;
            }
        }
        List<CssLinks> GetCssLinksHrefs(HtmlDocument document)
        {
            var linkTags = document.DocumentNode.Descendants("link").Select(e => new { rel = e.GetAttributeValue("rel", null), href = e.GetAttributeValue("href", null) });
            List<CssLinks> linksSource = new List<CssLinks>();
            foreach (var link in linkTags)
            {
                if (link.rel == "stylesheet")
                    linksSource.Add(new CssLinks { Css = link.href });
            }
            return linksSource.Distinct().ToList();
        }
        List<ImgSources> GetImgSources(HtmlDocument document)
        {
            var srcs = document.DocumentNode.Descendants("img")
                                            .Select(e => e.GetAttributeValue("src", null))
                                            .Where(s => !String.IsNullOrEmpty(s));
            List<ImgSources> linksSource = new List<ImgSources>();
            foreach (var scr in srcs)
            {
                linksSource.Add(new ImgSources { Scr = scr });
            }
            return linksSource.Distinct().ToList();
        }
        List<string> GetHrefs(HtmlDocument document, string url)
        {
            var hrefs = document.DocumentNode.Descendants("a")
                                           .Select(e => e.GetAttributeValue("href", null))
                                           .Where(u => !String.IsNullOrEmpty(u)).ToList();
            hrefs = UrlsClean(hrefs, url);
            return hrefs.Distinct().ToList();
        }
        public List<string> UrlsClean(List<string> hrefs, string url)
        {
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
        List<InternalUrls> GetUrlsInternal(List<string> hrefs, string url)
        {
            Uri myUri = new Uri(url);
            var protocol = myUri.Scheme;
            string host = myUri.Host;
            var temp = protocol + "://" + host;
            List<InternalUrls> urlInternalList = new List<InternalUrls>();
            foreach (var href in hrefs)
            {
                if (href.StartsWith(temp))
                    urlInternalList.Add(new InternalUrls { Url = href });
                if (href.StartsWith("//" + host))
                    urlInternalList.Add(new InternalUrls { Url = protocol + ":" + href });
                if (href.StartsWith("/") && !href.StartsWith("//"))
                    urlInternalList.Add(new InternalUrls { Url = temp + href });
            }
            return urlInternalList.Distinct().ToList();
        }
        List<ExternalUrls> GetUrlsExtrnal(List<string> hrefs, string url)
        {
            List<ExternalUrls> urlExternallList = new List<ExternalUrls>();
            Uri myUri = new Uri(url);
            var protocol = myUri.Scheme;
            string host = myUri.Host;
            var temp = protocol + "://" + host;
            foreach (var href in hrefs)
            {
                if ((href.StartsWith("http://") || href.StartsWith("https://")) && !href.StartsWith(temp))
                    urlExternallList.Add(new ExternalUrls { Url = href });
                if (href.StartsWith("//") && !href.StartsWith("//" + host))
                    urlExternallList.Add(new ExternalUrls { Url = protocol + ":" + href });
            }
            return urlExternallList.Distinct().ToList();
        }
        public string HostFullAdr(string url)
        {
            try
            {
                var myUri = new Uri(url);
                var protocol = myUri.Scheme;
                var host = myUri.Host;
                return protocol + "://" + host;
            }
            catch (Exception ex)
            {
                logger.Error(url + ex.Message + ex.InnerException);
                return "Uri ex";
            }
            
        }
    }
}
