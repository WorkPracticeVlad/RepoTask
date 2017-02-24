using System;
//using NUnit.Framework;
using ClassLibrary.FirstTask;
using Moq;
using ClassLibrary.Data;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Concurrent;

namespace UnitTestProject
{
    [TestClass]
    public class UnitMoq
    {
        [TestMethod]
        public void Dispose_IsCalled_OnceCrawling()
        {
            var moqRepo = new Mock<IRepository>();
            var crawler = new Crawling(moqRepo.Object);
            crawler.Dispose();
            moqRepo.Verify(i => i.Dispose(), Times.Once());
        }
        [TestMethod]
        public void GetHostIdIfExist_getId_20()
        {
            var moqRepo = new Mock<IRepository>();
            var idHost = 20;
            moqRepo.Setup(r => r.GetHostIdIfExist(It.IsAny<string>())).Returns(idHost);
            int? temp = moqRepo.Object.GetHostIdIfExist(It.IsAny<string>());
            Assert.AreEqual((int)temp, idHost);
        }
        [TestMethod]
        public void GetHostIdIfExist_getId_null()
        {
            var moqRepo = new Mock<IRepository>();
            int? temp = moqRepo.Object.GetHostIdIfExist(It.IsAny<string>());
            Assert.AreEqual(temp, null);
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WrongPathWatch()
        {
            var moqWatcher = new Mock<WatcherForCommand>();
            moqWatcher.Object.StartWatch("path", "path");
        }
        [TestMethod]
        public void AddHostConnection_Add_NoSave()
        {
            var urlsSave = new ConcurrentDictionary<string, bool>();
            urlsSave.TryAdd("Url", false);
            var hosts = new ConcurrentDictionary<string, int>();
            hosts.TryAdd("host", 0);
            var pages = new PageUrls[] { new PageUrls() { Url = "Url" } };
            var moqRepo = new Mock<IRepository>();
            moqRepo.Setup(r => r.GetHostIdIfExist(It.IsAny<string>())).Returns(It.IsAny<int>());
            Helper.AddHostConnection(pages, urlsSave, hosts, new Measures(), moqRepo.Object);
            moqRepo.Verify(r => r.AddHost(It.IsAny<string>()), Times.Never());
            moqRepo.Verify(r => r.GetHostIdIfExist(It.IsAny<string>()), Times.Once());
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]      
        public void WriteTree_GetUrlsForHost_NoMatchDB()
        {
            var expected = "This is tree\r\nno match in DB\r\n";
            var moqRepo = new Mock<IRepository>();
            moqRepo.Setup(r => r.GetUrlsForHost(It.IsAny<string>())).Returns(new List<string>());
            var siteTreeBuilder = new SiteTreeBuilder(moqRepo.Object);
            var temp = siteTreeBuilder.WriteTree("path", It.IsAny<string>());   
            Assert.AreEqual(expected, temp);
        }
        [TestMethod]
        public void CountForSave_Dict_Conut()
        {
            var expected = 2;
            var dictionaryTest = new ConcurrentDictionary<string, bool>();
            dictionaryTest.TryAdd("q", true);
            dictionaryTest.TryAdd("w", false);
            dictionaryTest.TryAdd("e", false);
            var count = Helper.CountForSave(dictionaryTest);
            Assert.AreEqual(expected, count);
        }
    }
}
