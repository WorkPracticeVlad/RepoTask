using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ClassLibrary.FirstTask;
using HtmlAgilityPack;
using System.Linq;
using System.Collections.Generic;
using ClassLibrary.Data;

namespace UnitTestProject
{
    [TestClass]
    public class UnitTest1
    {       
        Measures m = new Measures();
        [TestMethod]
        public void HostFullAdr_Url_Host()
        {
            string url = "http://stackoverflow.com/questions/16330404/how-to-remove-remote-origin-from-git-repo";
            string expected = "http://stackoverflow.com";
            Assert.AreEqual(expected, m.HostFullAdr(url));
        }
        [TestMethod]
        public void UrlsClean_ListUrl_CleanList()
        {
            List<string> urls = new List<string>()
        {
            "/",
            "/forum/register",
            "http://www.sql.ru/forum/programming",
            "/courses",
            "http://javascript.ru/forum",
            "http://es5.javascript.ru",
            "#somewhere",
            "somescritp(){}",
            "javascript(0)",
            "",
            "https://learn.javascript.ru/",
            "https://learn.javascript.ru/prototypes",
            "https://learn.javascript.ru/prototypes"
        };
            List<string> expected = new List<string>()
        {
            "/",
            "/forum/register",
            "http://www.sql.ru/forum/programming",
            "/courses",
            "http://javascript.ru/forum",
            "http://es5.javascript.ru",
            "https://learn.javascript.ru/",
            "https://learn.javascript.ru/prototypes",
            "https://learn.javascript.ru/prototypes"
        };
            CollectionAssert.AreEqual(expected, m.UrlsClean(urls));
        }
    }
}
