using System;
using System.Collections.Generic;
using Crossroads.Utilities.Services;
using NUnit.Framework;

namespace Crossroads.Utilities.Test.Services
{
    public class HtmlElementTest
    {

        [Test]
        public void testBasicHtmlElement()
        {
            HtmlElement table = new HtmlElement("table");
            Assert.AreEqual(table.Build(), "<table></table>");
        }

        [Test]
        public void testTextNode()
        {
            HtmlElement td = new HtmlElement("td", "test");
            Assert.AreEqual(td.Build(), "<td>test</td>");
        }

        [Test]
        public void testNestedNodesWithList()
        {
            HtmlElement td = new HtmlElement("td", "test");
            HtmlElement td2 = new HtmlElement("td", "test2");
            HtmlElement tr = new HtmlElement("tr", new List<HtmlElement>(){ td, td2 });
            Assert.AreEqual("<tr><td>test</td><td>test2</td></tr>", tr.Build());

        }

        [Test]
        public void testNestedNodesWithAppend()
        {
            HtmlElement table = new HtmlElement("table");
            HtmlElement td = new HtmlElement("td", "test");
            HtmlElement td2 = new HtmlElement("td", "test2");
            HtmlElement tr = new HtmlElement("tr", new List<HtmlElement>() { td, td2 });
            table = table.Append(tr);
            Assert.AreEqual("<table><tr><td>test</td><td>test2</td></tr></table>", table.Build());
        }

        [Test]
        public void testAppendListOfElements()
        {            
            HtmlElement td = new HtmlElement("td", "test");
            HtmlElement td2 = new HtmlElement("td", "test2");
            var lst = new List<HtmlElement>() {td, td2};
            HtmlElement tr = new HtmlElement("tr");
            tr.Append(lst);
            Assert.AreEqual("<tr><td>test</td><td>test2</td></tr>", tr.Build());
        }

        [Test]
        public void testAttributes()
        {
            var attrs = new Dictionary<String, String>()
            {
                {"class", "my-class and-another"},
                {"ng-value", "blah"}
            };
            HtmlElement td = new HtmlElement("td", attrs);
            Assert.AreEqual("<td class='my-class and-another' ng-value='blah' ></td>", td.Build());
        }

        [Test]
        public void testAnoymousFunc()
        {
            var table = new HtmlElement("table", new Dictionary<string, string>() {{"width", "100%"}})
                .Append( ()=> 
                    new HtmlElement("tr")
                    .Append(new HtmlElement("td", "test", new Dictionary<string, string>() {{"class", "my-class"}}))
                    .Append(new HtmlElement("td", "test2", new Dictionary<string, string>() {{"class", "my-class"}}))
                 );
            Assert.AreEqual("<table width='100%' ><tr><td class='my-class' >test</td><td class='my-class' >test2</td></tr></table>", table.Build());
        }

        [Test]
        public void testComposingElements()
        {
            var table = new HtmlElement("table", new Dictionary<string, string>() {{"width", "100%"}})
                .Append(new HtmlElement("tr")
                    .Append(new HtmlElement("td", "test", new Dictionary<string, string>() {{"class", "my-class"}}))
                    .Append(new HtmlElement("td", "test2", new Dictionary<string, string>() {{"class", "my-class"}}))
                );
            Assert.AreEqual("<table width='100%' ><tr><td class='my-class' >test</td><td class='my-class' >test2</td></tr></table>", table.Build());
        }

    }
}