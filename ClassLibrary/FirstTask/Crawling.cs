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
        void CrawlOneUrlProducer(string fullUrl)
        {
            if (cancelTicket)
                return;
            var result = measures.TakeMesuares(fullUrl);
            dicitonaryOfCrawledUrls.TryAdd(result.Url, result);
            urlsForCheck.TryAdd(result.Url, result.Url);
        }
        void CrawlInternalUrls(PageUrls page)
        {
            if (page.InternalUrls != null)
            {
                foreach (var url in page.InternalUrls)
                {
                    if (cancelTicket)
                        return;
                    if (!urlsForCheck.ContainsKey(url.Url))
                        CrawlOneUrlProducer(url.Url);                 
                }
            }
        }
        void CrawlExternalUrls(PageUrls page)
        {
            if (page.ExternalUrls != null)
            {
                foreach (var url in page.ExternalUrls)
                {
                    if (cancelTicket)
                        return;
                    if (!urlsForCheck.ContainsKey(url.Url))
                        CrawlOneUrlProducer(url.Url); 
                }
            }
        }
        void OneThreadProducer(string url)
        {
            int threadName = Int32.Parse(Thread.CurrentThread.Name);
            if (!urlsForCheck.ContainsKey(url))
                CrawlOneUrlProducer(url);
            if (!dicitonaryOfCrawledUrls.IsEmpty)
            {
                if (threadName % 2 == 0)
                    CrawlInternalUrls(dicitonaryOfCrawledUrls.ElementAt(0).Value);
                CrawlExternalUrls(dicitonaryOfCrawledUrls.ElementAt(0).Value);
            }
            while (!dicitonaryOfCrawledUrls.IsEmpty)
            {
                if (cancelTicket)
                    break;
                if (dicitonaryOfCrawledUrls.Count <= threadName)
                    continue;
                var temp = dicitonaryOfCrawledUrls.ElementAtOrDefault(threadName);
                if (temp.Value != null)
                    CrawlUrls(temp.Value);
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
                    Thread.Sleep(2000);
                    if (cancelTicket)
                        break;                   
                    if (Helper.CountForSave(urlsForDBSaving) != 0)
                    {                   
                        PageUrls[] items = new PageUrls[Helper.CountForSave(urlsForDBSaving)];
                        Helper.RemoveFromDict(items, dicitonaryOfCrawledUrls, urlsForDBSaving);
                        Helper.AddHostConnection(items, urlsForDBSaving, hosts, measures, _repo);
                        _repo.AddPageOrUpdate(items.ToList());
                        logger.Trace("Save or update to DB " + items.Length + " pages");
                    }                        
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
        }     
        public void Dispose()
        {
            _repo.Dispose();
        }
    }
}
