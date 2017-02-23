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
    public class UnitRepUsePath
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
        public void AddHostConnection_Add_Save()
        {
            var urlsSave= new ConcurrentDictionary<string, bool>();
            urlsSave.TryAdd("Url", false);
            var pages = new PageUrls[]{ new PageUrls() {Url="Url" } };
            var moqRepo = new Mock<IRepository>();
            var crawl = new Crawling(moqRepo.Object);           
            moqRepo.Setup(r => r.GetHostIdIfExist(It.IsAny<string>())).Returns(It.IsAny<int>());
            crawl.AddHostConnection(pages, urlsSave);
            moqRepo.Verify(r=>r.AddHost(It.IsAny<string>()), Times.Never());
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]      
        public void WriteTree_GetUrlsForHost()
        {
            var moqRepo = new Mock<IRepository>();
            var siteTreeBuilder = new SiteTreeBuilder(moqRepo.Object);
            siteTreeBuilder.WriteTree("path", It.IsAny<string>());
            moqRepo.Setup(r => r.GetUrlsForHost(It.IsAny<string>())).Returns(new List<string>() { { "string" } });     
        }
    }
}
