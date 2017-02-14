using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClassLibraryForTask
{
    public class Crawling
    {
        public Crawling(IRepository rep)
        {
            _rep = rep;
        }
        IRepository _rep;
        static List<string> urls = new List<string>();
        static Queue<PageUrl> sharedQueue = new Queue<PageUrl>();
        public void StartCrawl(string url, int threadsCount)
        {
            Thread[] threads = new Thread[threadsCount];
            var halfForEnqueue = threads.Length / 2;
            for (int i = 0; i < threads.Length; i++)
            {
                if (i < halfForEnqueue)
                    threads[i] = new Thread(() => StartCrawlOneThread(url));
                else if (i < threadsCount)
                    threads[i] = new Thread(Consumer);
                threads[i].Start();
            }
            for (int i = 0; i < threads.Length; i++)
            {
                threads[i].Join();
            }
        }
        PageUrl CrawlOneUrlProducer(string fullUrl)
        {
            Mesuares m = new Mesuares();
            var res = new PageUrl();
            lock (sharedQueue)
            {
                if (!urls.Any(r => r == fullUrl) && !sharedQueue.Any(r => r.Url == fullUrl))
                {
                    res = m.TakeMesuares(fullUrl);
                    sharedQueue.Enqueue(res);
                    Monitor.Pulse(sharedQueue);
                    Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " Put in queue");
                }
            }
            return res;
        }
        void CrawlInternalUrls(string url)
        {
            var internalUrls = _rep.GetInternalUrl(url);
            if (internalUrls != null)
            {
                foreach (var urlIn in internalUrls)
                {
                    CrawlOneUrlProducer(urlIn);
                }
            }
        }
        void CrawlExternalUrls(string url)
        {
            var externalUrls = _rep.GetInternalUrl(url);
            if (externalUrls != null)
            {
                foreach (var urlIn in externalUrls)
                {
                    CrawlOneUrlProducer(urlIn);
                }
            }
        }
        void StartCrawlOneThread(string url)
        {
            var cr = new Crawling(_rep);
            var r = cr.CrawlOneUrlProducer(url);
            //cr.CrawlInternalUrls(r.Url);
            //cr.CrawlExternalUrls(r.Url);
            for (int i = 0; i < urls.Count; i++)
            {
                var temp = urls[i];
                if (Thread.CurrentThread.ManagedThreadId % 2 == 0)
                {
                    cr.CrawlInternalUrls(temp);
                    cr.CrawlExternalUrls(temp);
                }
                else
                {
                    cr.CrawlExternalUrls(temp);
                    cr.CrawlInternalUrls(temp);
                }
                if (i == 2)
                {
                    int x = i;
                }
            }
        }
        void Consumer()
        {
            while (true)
            {
                try
                {
                    lock (sharedQueue)
                    {
                        while (sharedQueue.Count == 0)
                            Monitor.Wait(sharedQueue);
                        var res = sharedQueue.Dequeue();
                        _rep.Add(res);
                        urls.Add(res.Url);
                        Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " Processing url: " + res.Url);
                    }
                }
                catch (DbEntityValidationException dbEx)
                {
                    foreach (var validationErrors in dbEx.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            Trace.TraceInformation("Property: {0} Error: {1}",
                                                    validationError.PropertyName,
                                                    validationError.ErrorMessage);
                        }
                    }
                }
            }
        }
    }
}
