using ClassLibrary.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.FirstTask
{
    public static class Helper
    {
        public static PageUrls[] AddHostConnection(PageUrls[] items, ConcurrentDictionary<string, bool> urlsForDBSaving, ConcurrentDictionary<string, int> hosts, Measures measures, IRepository repo)
        {
            RemoveNulls(items);
            for (int i = 0; i < items.Length; i++)
            {
                var hostName = measures.HostFullAdr(items[i].Url);
                if (hosts.ContainsKey(hostName))
                {
                    items[i].Fk_Hosts_Id = hosts[hostName];
                }
                else
                {
                    int? hostId = repo.GetHostIdIfExist(hostName);
                    if (hostId != null)
                    {
                        hosts.TryAdd(hostName, (int)hostId);
                        items[i].Fk_Hosts_Id = (int)hostId;
                    }
                    else
                    {
                        repo.AddHost(hostName);
                        hostId = (int)repo.GetHostIdIfExist(hostName);
                        hosts.TryAdd(hostName, (int)hostId);
                        items[i].Fk_Hosts_Id = (int)hostId;
                    }
                }
                urlsForDBSaving[items[i].Url] = true;
            }
            return items;
        }
        public static PageUrls[] RemoveFromDict(PageUrls[] items, ConcurrentDictionary<string, PageUrls> dicitonaryOfCrawledUrls, ConcurrentDictionary<string, bool> urlsForDBSaving)
        {
            for (int i = items.Length - 1; i >= 0; i--)
            {                
                    PageUrls temp;
                    dicitonaryOfCrawledUrls.TryGetValue(ItemsForSave(urlsForDBSaving).ElementAt(i).Key, out temp);
                    if (temp != null)
                    {
                        dicitonaryOfCrawledUrls.
                        TryRemove(temp.Url,
                       out items[i]);
                    }
            }
            return items;
        }
        public static int CountForSave(ConcurrentDictionary<string, bool> urlsForDBSaving)
        {
            return ItemsForSave(urlsForDBSaving).Count();
        }
        static IEnumerable<KeyValuePair<string, bool>> ItemsForSave(ConcurrentDictionary<string, bool> urlsForDBSaving)
        {
            return urlsForDBSaving.Where(u => u.Value == false);
        }
        static PageUrls[] RemoveNulls(PageUrls[] items)
        {
            return items.Where(c => c != null).ToArray();
        }
    }
}
