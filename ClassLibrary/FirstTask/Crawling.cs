using ClassLibrary.Data;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClassLibrary.FirstTask
{
    public class Crawling
    {
        Logger logger = LogManager.GetCurrentClassLogger();
        ConcurrentDictionary<string, PageUrls> dicitonaryOfCrawledUrls = new ConcurrentDictionary<string, PageUrls>();
        ConcurrentDictionary<string, string> urlsForCheck = new ConcurrentDictionary<string, string>();
        ConcurrentDictionary<string, bool> urlsForDBSaving = new ConcurrentDictionary<string, bool>();
        ConcurrentDictionary<string, int> hosts = new ConcurrentDictionary<string, int>();
        PageUrls result = new PageUrls();
        Measures measures = new Measures();
        bool flagDB = true;
        static bool cancelTicket = false;
        private IRepository _repo;
        public Crawling(IRepository repo)
        {
            _repo = repo;
        }
        public void StartCrawl(string url, int threadsCount)
        {
            flagDB = true;
            cancelTicket = false;
            System.Threading.Tasks.Task.Run(() => ConsumerDbContext());
            Thread[] threads = new Thread[threadsCount];
            for (int i = 0; i < threads.Length; i++)
            {
                threads[i] = new Thread(() => OneThreadProducer(url));
                threads[i].Name = i.ToString();
                threads[i].Start();
            }
            for (int i = 0; i < threads.Length; i++)
            {
                threads[i].Join();
            }
        }
        PageUrls CrawlOneUrlProducer(string fullUrl)
        {
            if (cancelTicket)
                return null;
            var result = measures.TakeMesuares(fullUrl);
            dicitonaryOfCrawledUrls.TryAdd(result.Url, result);
            urlsForCheck.TryAdd(result.Url, result.Url);
            return result;
        }
        void CrawlInternalUrls(PageUrls page)
        {
            if (page.InternalUrls != null)
            {
                foreach (var url in page.InternalUrls)
                {
                    if (!urlsForCheck.ContainsKey(url.Url))
                        CrawlOneUrlProducer(url.Url);
                    if (cancelTicket)
                        return;
                }
            }
        }
        void CrawlExternalUrls(PageUrls page)
        {
            if (page.ExternalUrls != null)
            {
                foreach (var url in page.ExternalUrls)
                {
                    if (!urlsForCheck.ContainsKey(url.Url))
                        CrawlOneUrlProducer(url.Url);
                    if (cancelTicket)
                        return;
                }
            }
        }
        void OneThreadProducer(string url)
        {
            int threadName = Int32.Parse(Thread.CurrentThread.Name);
            if (!urlsForCheck.ContainsKey(url))
                result = CrawlOneUrlProducer(url);
            if (!dicitonaryOfCrawledUrls.IsEmpty)
            {
                if (threadName % 2 == 0)
                    CrawlInternalUrls(dicitonaryOfCrawledUrls.ElementAt(0).Value);
                CrawlExternalUrls(dicitonaryOfCrawledUrls.ElementAt(0).Value);
            }
            while (!dicitonaryOfCrawledUrls.IsEmpty)
            {
                if (dicitonaryOfCrawledUrls.Count <= threadName)
                    continue;
                var temp = dicitonaryOfCrawledUrls.ElementAtOrDefault(threadName);
                if (temp.Value != null)
                    CrawlUrls(temp.Value);
                if (cancelTicket)
                    break;
            }
            logger.Trace("Thread " + Thread.CurrentThread.Name + " finish");
        }
        private void CrawlUrls(PageUrls resultOfmeasure)
        {
            CrawlInternalUrls(resultOfmeasure);
            CrawlExternalUrls(resultOfmeasure);
            if (cancelTicket)
                return;
            if (!urlsForDBSaving.ContainsKey(resultOfmeasure.Url))
            {
                urlsForDBSaving.TryAdd(resultOfmeasure.Url, false);
            }
        }
        void ConsumerDbContext()
        {
            while (flagDB)
            {
                while (!dicitonaryOfCrawledUrls.IsEmpty)
                {
                    if (Helper.CountForSave(urlsForDBSaving) != 0)
                    {
                        PageUrls[] items = new PageUrls[urlsForDBSaving.Where(u => u.Value == false).Count()];
                        items = Helper.RemoveFromDict(items, dicitonaryOfCrawledUrls, urlsForDBSaving);
                        items = Helper.AddHostConnection(items, urlsForDBSaving, hosts, measures, _repo);
                        _repo.AddPageOrUpdate(items.ToList());
                        logger.Trace("Save or update to DB " + items.Length + " pages");
                    }
                    if (cancelTicket)
                        break;
                    flagDB = false;
                }
                if (cancelTicket)
                    break;
            }
            Clear();
            logger.Trace("Finish DB");
        }
        public void Cancel()
        {
            cancelTicket = true;
            Thread.Sleep(750);
            Clear();
        }
        void Clear()
        {
            dicitonaryOfCrawledUrls.Clear();
            urlsForCheck.Clear();
            urlsForDBSaving.Clear();
            hosts.Clear();
            result = null;
        }
        public PageUrls[] AddHostConnection(PageUrls[] items, ConcurrentDictionary<string, bool> urlsForDBSaving)//, ConcurrentDictionary<string, int> hosts, IRepository repo)
        {
            items = items.Where(c => c != null).ToArray();
            for (int i = 0; i < items.Length; i++)
            {
                var hostName = measures.HostFullAdr(items[i].Url);
                if (hosts.ContainsKey(hostName))
                {
                    items[i].Fk_Hosts_Id = hosts[hostName];
                }
                else
                {
                    int? hostId = _repo.GetHostIdIfExist(hostName);
                    if (hostId != null)
                    {
                        hosts.TryAdd(hostName, (int)hostId);
                        items[i].Fk_Hosts_Id = (int)hostId;
                    }
                    else
                    {
                        _repo.AddHost(hostName);
                        hostId = (int)_repo.GetHostIdIfExist(hostName);
                        hosts.TryAdd(hostName, (int)hostId);
                        items[i].Fk_Hosts_Id = (int)hostId;
                    }
                }
                urlsForDBSaving[items[i].Url] = true;
            }
            return items;
        }
        public void Dispose()
        {
            _repo.Dispose();
        }
    }
}
