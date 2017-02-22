using System;
//using NUnit.Framework;
using ClassLibrary.FirstTask;
using Moq;
using ClassLibrary.Data;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject
{
    [TestClass]
    public class UnitTest2
    {
        [TestMethod]
        public void Dispose_IsCalled_OnceCrawling()
        {
            var moqRepo = new Mock<IRepository>();
            var crawler = new Crawling(moqRepo.Object);
            crawler.Dispose();
            moqRepo.Verify(i=>i.Dispose(), Times.Once());
        }
        [TestMethod]
        public void GetHostIdIfExist_getId_20()
        {
             var moqRepo = new Mock<IRepository>();
            moqRepo.Setup(r => r.GetHostIdIfExist("https://github.com")).Returns(20);
            int? temp= moqRepo.Object.GetHostIdIfExist("https://github.com");
            Assert.AreEqual((int)temp, 20);
        }
    }
}
