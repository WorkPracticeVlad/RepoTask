using ConsoleApp.Data;
using ConsoleApp.Task;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;


namespace ConsoleApp.Task
{
    class Crawling
    {
        ConcurrentDictionary<string, PageUrls> dicitonaryOfCrawledUrls = new ConcurrentDictionary<string, PageUrls>();
        ConcurrentDictionary<string, string> urls = new ConcurrentDictionary<string, string>();
        ConcurrentDictionary<string, bool> urlsDB = new ConcurrentDictionary<string, bool>();
        BlockingCollection<Queue<PageUrls>> listOfQueue = new BlockingCollection<Queue<PageUrls>>();
        ConcurrentDictionary<string, int> hosts = new ConcurrentDictionary<string, int>();
        Measures m = new Measures();
        bool flagDB = true;
        bool flagQueue = true;
        private IRepository _repo;
        public Crawling(IRepository repo)
        {
            _repo = repo;
        }
        public void StartCrawl(string url, int threadsCount)
        {
            System.Threading.Tasks.Task.Run(() => ConsumerDbContext());
            Thread[] threads = new Thread[threadsCount];
            int halfOfThreads = threads.Length / 2;
            for (int i = 0; i < halfOfThreads; i++)
            {
                listOfQueue.Add(new Queue<PageUrls>());
            }
            for (int i = 0; i < halfOfThreads; i++)
            {
                var iteratorQueue = listOfQueue.ElementAt(i);
                threads[i] = new Thread(() => CrawlOneThread(url, iteratorQueue));
                threads[i].Name = i.ToString();
                threads[i + halfOfThreads] = new Thread(() => Consumer(iteratorQueue));
                threads[i].Start();
                threads[i + halfOfThreads].Start();
            }
            if (threads.Length % 2 != 0)
            {
                var iteratorQueue = listOfQueue.ElementAt(0);
                threads[threads.Length - 1] = new Thread(() => CrawlOneThread(url, iteratorQueue));
                threads[threads.Length - 1].Name = (threads.Length - 1).ToString();
                threads[threads.Length - 1].Start();
            }

            for (int i = 0; i < threads.Length; i++)
            {
                threads[i].Join();
            }
        }
        PageUrls CrawlOneUrlProducer(string fullUrl, Queue<PageUrls> queue)
        {
            Measures m = new Measures();
            var res = new PageUrls();
            lock (queue)
            {
                if (!urls.ContainsKey(fullUrl) && !queue.Any(r => r.Url == fullUrl))
                {
                    res = m.TakeMesuares(fullUrl);
                    queue.Enqueue(res);
                    Monitor.Pulse(queue);
                }
            }
            return res;
        }
        void CrawlInternalUrls(PageUrls r, Queue<PageUrls> queue)
        {
            if (r.InternalUrls != null)
            {
                foreach (var url in r.InternalUrls)
                {
                    if (!urls.ContainsKey(url.Url))
                        CrawlOneUrlProducer(url.Url, queue);
                }
            }
        }
        void CrawlExternalUrls(PageUrls r, Queue<PageUrls> queue)
        {
            if (r.ExternalUrls != null)
            {
                foreach (var url in r.ExternalUrls)
                {
                    if (!urls.ContainsKey(url.Url))
                        CrawlOneUrlProducer(url.Url, queue);
                }
            }
        }
        void CrawlOneThread(string url, Queue<PageUrls> queue)
        {
            int threadName = Int32.Parse(Thread.CurrentThread.Name);
            var res = CrawlOneUrlProducer(url, queue);
            CrawlInternalUrls(res, queue);
            CrawlExternalUrls(res, queue);
            if (!urlsDB.ContainsKey(res.Url))
            {
                urlsDB.TryAdd(res.Url, false);
            }
            while (!dicitonaryOfCrawledUrls.IsEmpty)
            {
                var temp = dicitonaryOfCrawledUrls.ElementAt(threadName);
                CrawlInternalUrls(temp.Value, queue);
                CrawlExternalUrls(temp.Value, queue);
                if (!urlsDB.ContainsKey(temp.Key))
                {
                    urlsDB.TryAdd(temp.Key, false);
                }             
            }
        }
        void Consumer(Queue<PageUrls> queue)
        {
            while (flagQueue)
            {
                lock (queue)
                {
                    while (queue.Count == 0)
                        Monitor.Wait(queue);
                    var res = queue.Dequeue();
                    string temp = res.Url;
                    if (!urls.ContainsKey(temp) )/*&& !dicitonaryOfCrawledUrls.Any(r => r.Key == temp))*/
                    {
                        dicitonaryOfCrawledUrls.TryAdd(res.Url, res);
                        urls.TryAdd(temp, temp);
                        Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " Processing to list url: " + res.Url);
                    }                  
                }
                if (dicitonaryOfCrawledUrls.Count != 0)
                {
                    while (dicitonaryOfCrawledUrls.Count!=0)
                    {
                        lock (queue)
                        {
                            while (queue.Count == 0)
                                Monitor.Wait(queue);
                            var res = queue.Dequeue();
                            string temp = res.Url;
                            if (!urls.ContainsKey(temp) )/*&& !dicitonaryOfCrawledUrls.Any(r => r.Key == temp))*/
                            {
                                dicitonaryOfCrawledUrls.TryAdd(res.Url, res);
                                urls.TryAdd(temp, temp);
                                Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " Processing to list url: " + res.Url);
                            }                         
                        }
                    }
                    flagQueue = false;
                }
                          
            }
        }
        void ConsumerDbContext()
        {
            while (flagDB)
            {
                while (dicitonaryOfCrawledUrls.Count != 0)
                {
                    if (urlsDB.Where(u => u.Value == false).Count()!=0)
                    {
                        PageUrls[] items = new PageUrls[urlsDB.Where(u=>u.Value==false).Count()];
                       
                        for (int i = items.Length - 1; i >= 0; i--)
                        {
                            if (dicitonaryOfCrawledUrls.ContainsKey(urlsDB.Where(u => u.Value == false).ElementAt(i).Key))
                            {
                                var temp = dicitonaryOfCrawledUrls.FirstOrDefault(p => p.Key == urlsDB.Where(u => u.Value == false).ElementAt(i).Key).Key;
                                if (temp!=null)
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
                            if (hosts.ContainsKey(m.HostFullAdr(items[i].Url)))
                            {
                                int fk = hosts[m.HostFullAdr(items[i].Url)];
                            }
                            else
                            {
                                int v = hosts.Count + 1;
                                string k = m.HostFullAdr(items[i].Url);
                                hosts.TryAdd(k, v);
                            }
                            urlsDB[items[i].Url] = true;
                        }
                        if (items.Length!=0)
                        {
                            //_repo.Add(items.ToList());
                            Console.WriteLine("Saving to DB ");
                        }                       
                        Thread.Sleep(10000);
                    }
                    flagDB = false;
                }
            }
        }
    }
}

