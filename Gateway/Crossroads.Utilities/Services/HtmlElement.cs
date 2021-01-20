using System;
using System.Collections.Generic;
using System.Linq;

namespace Crossroads.Utilities.Services
{
    /// <summary>
    /// A helper class for create HTML nodes and documents. 
    /// Create an HtmlElement by calling one of the contructors. 
    /// Two methods exist other than  the contructors... Append() and Build()
    /// Append will add an HtmlElement to the current instance and return the 
    /// current element for composability.
    /// Build returns the string representation of the Html Node
    /// </summary>
    public class HtmlElement
    {

        private readonly String el;
        private readonly String elementStart;
        private readonly String elementText = String.Empty;
        private readonly String elementEnd;
        private readonly List<HtmlElement> childrenElements = new List<HtmlElement>();

        /// <summary>
        /// The simplest constructor. It only takes an element name
        /// ex. new HtmlElement('td') 
        /// </summary>
        /// <param name="el">The element to construct</param>
        public HtmlElement(String el)
        {
            this.el = el;
            this.elementStart = "<" + el + ">";
            this.elementEnd = "</" + el + ">";
        }

        /// <summary>
        /// Builds the specified element and adds the 
        /// text inside. 
        /// ex. new HtmlElement('td', 'test') represents <td>test</td>
        /// </summary>
        /// <param name="el">The element name </param>
        /// <param name="text">The text to put inside the element</param>
        public HtmlElement(String el, String text) : this(el)
        {
            this.elementText = text;
        }

        /// <summary>
        /// Builds the specified element and adds the 
        /// child element inside. 
        /// ex. new HtmlElement('td', new HtmlELement("p")) represents <td><p></p></td>
        /// </summary>
        /// <param name="el">The element name </param>
        /// <param name="child">The child htmlelement to put inside the element</param>
        public HtmlElement(String el, HtmlElement child) : this(el)
        {
            this.childrenElements = new List<HtmlElement>() { child };
        }

        /// <summary>
        /// Builds the element and adds a list of children elements
        /// </summary>
        /// <param name="el">The element</param>
        /// <param name="children">A list of HtmlElements to add as children</param>
        public HtmlElement(String el, List<HtmlElement> children) : this(el)
        {
            this.childrenElements = children;
        }

        /// <summary>
        /// Builds the element with a list of attributes
        /// </summary>
        /// <param name="el">The element</param>
        /// <param name="attributes">A dictionary of attributes to add to the element</param>
        public HtmlElement(String el, Dictionary<String, String> attributes )
        {
            var attrs = attributes.Aggregate("", (current, item) => current + (item.Key + "='" + item.Value + "' " ));
            this.elementStart = "<" + el + " " + attrs + ">";
            this.elementEnd = "</" + el + ">";
            this.el = el;
        }

        /// <summary>
        /// Builds the element with a list of attributes and the text to put inside the element
        /// </summary>
        /// <param name="el">The element </param>
        /// <param name="attributes">The list of attributes</param>
        /// <param name="text">Text that belongs inside of the element</param>
        public HtmlElement(String el, Dictionary<string, string> attributes, String text) : this(el, attributes)
        {
            this.elementText = text;
        }

        /// <summary>
        /// Builds the element with a list of attributes and text
        /// </summary>
        /// <param name="el">The element</param>
        /// <param name="text">The text to insert into the element</param>
        /// <param name="attributes">A dictionary of attributes to add</param>
        public HtmlElement(String el, String text, Dictionary<String, String> attributes) 
            : this(el, attributes)
        {
            this.elementText = text;
        }

        /// <summary>
        /// Builds the element with a list of children elements and a list of attributes
        /// </summary>
        /// <param name="el">The element</param>
        /// <param name="children">A list of children elements</param>
        /// <param name="attributes">A list of attributes</param>
        public HtmlElement(String el, List<HtmlElement> children, Dictionary<String, String> attributes)
            : this(el, attributes)
        {
            this.childrenElements = children;
        }
        
        /// <summary>
        /// Append an element to this one
        /// </summary>
        /// <param name="el">The element to append</param>
        /// <returns>An HtmlElement after appending the requested Element</returns>
        public HtmlElement Append(HtmlElement el)
        {
            this.childrenElements.Add(el);
            return this;
        }

        /// <summary>
        /// Append an element to this
        /// </summary>
        /// <param name="createElement"> A function that returns an HtmlElement</param>
        /// <returns>An HtmlElement after appending the requested Element</returns>
        public HtmlElement Append(Func<HtmlElement> createElement)
        {
            return Append(createElement());
        }

        /// <summary>
        /// Append a list of elements to this
        /// </summary>
        /// <param name="element">The list of elements</param>
        /// <returns>An HtmlElement after appending the requested Element</returns>
        public HtmlElement Append(List<HtmlElement> element)
        {
            this.childrenElements.AddRange(element);
            return this;
        } 

        /// <summary>
        /// Returns a string represention of the HtmlElement and all child elements
        /// </summary>
        /// <returns>The html representation fo the HtmlElement</returns>
        public String Build()
        {
            String element = this.childrenElements.Aggregate("", (current, child) => current + child.Build());
            return this.elementStart + this.elementText +  element + this.elementEnd;
        }

    }
}