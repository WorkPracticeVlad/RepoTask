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
    class Crawling
    {
        Logger lgr = LogManager.GetCurrentClassLogger();
        ConcurrentDictionary<string, PageUrls> dicitonaryOfCrawledUrls = new ConcurrentDictionary<string, PageUrls>();
        ConcurrentDictionary<string, string> urls = new ConcurrentDictionary<string, string>();
        ConcurrentDictionary<string, bool> urlsForDBSaving = new ConcurrentDictionary<string, bool>();
        ConcurrentDictionary<string, int> hosts = new ConcurrentDictionary<string, int>();
        PageUrls result = new PageUrls();
        Measures m = new Measures();
        int hostCount = 0;
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
            var result = m.TakeMesuares(fullUrl);
            dicitonaryOfCrawledUrls.TryAdd(result.Url, result);
            urls.TryAdd(result.Url, result.Url);
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " processing to list url: " + result.Url);
            return result;
        }
        void CrawlInternalUrls(PageUrls page)
        {
            if (page.InternalUrls != null)
            {
                foreach (var url in page.InternalUrls)
                {
                    if (!urls.ContainsKey(url.Url))
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
                    if (!urls.ContainsKey(url.Url))
                        CrawlOneUrlProducer(url.Url);
                    if (cancelTicket)
                        return;
                }
            }
        }
        void OneThreadProducer(string url)
        {
            int threadName = Int32.Parse(Thread.CurrentThread.Name);
            if (!urls.ContainsKey(url))
                result = CrawlOneUrlProducer(url);
            CrawlUrls(dicitonaryOfCrawledUrls.ElementAt(0).Value);
            while (!dicitonaryOfCrawledUrls.IsEmpty)
            {
                var temp = dicitonaryOfCrawledUrls.ElementAtOrDefault(threadName);
                if (temp.Value != null)
                    CrawlUrls(temp.Value);
                if (cancelTicket)
                    break;
            }
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
                while (dicitonaryOfCrawledUrls.Count != 0)
                {
                    if (urlsForDBSaving.Where(u => u.Value == false).Count() != 0)
                    {
                        PageUrls[] items = new PageUrls[urlsForDBSaving.Where(u => u.Value == false).Count()];
                        for (int i = items.Length - 1; i >= 0; i--)
                        {
                            if (dicitonaryOfCrawledUrls.ContainsKey(urlsForDBSaving.Where(u => u.Value == false).ElementAt(i).Key))
                            {
                                var temp = dicitonaryOfCrawledUrls.FirstOrDefault(p => p.Key == urlsForDBSaving.Where(u => u.Value == false).ElementAt(i).Key).Key;
                                if (temp != null)
                                {
                                    dicitonaryOfCrawledUrls.
                                    TryRemove(temp,
                                   out items[i]);
                                }
                            }
                        }
                        items = items.Where(c => c != null).ToArray();
                        for (int i = 0; i < items.Length; i++)
                        {
                            var hostName = m.HostFullAdr(items[i].Url);
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
                                    hostCount = _repo.GetHostsCount();
                                    hostCount++;
                                    _repo.AddHost(hostCount, hostName);
                                    hosts.TryAdd(hostName, hostCount);
                                    items[i].Fk_Hosts_Id = hostCount;
                                }
                            }
                            urlsForDBSaving[items[i].Url] = true;
                        }
                        _repo.AddPageOrUpdate(items.ToList());
                        Console.WriteLine("\n Saving to DB \n");
                    }
                    if (cancelTicket)
                        break;
                    flagDB = false;
                }
                if (cancelTicket)
                    break;
            }
            // Clear();
            //_repo.Dispose();
            lgr.Trace("Finish");
        }
        public void Cancel()
        {
            //_repo.Dispose();
            Clear();
            cancelTicket = true;
        }
        void Clear()
        {
            dicitonaryOfCrawledUrls.Clear();
            urls.Clear();
            urlsForDBSaving.Clear();
            hosts.Clear();
            hostCount = 0;
            result = null;
        }
    }
}
