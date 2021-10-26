using System;
using System.Collections.Generic;
using System.Text;

namespace SharedHtml
{
    public delegate void HtmlRetrieved();
    public sealed class HtmlData
    {
        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static HtmlData()
        {
            // honestly, idk, read more here 
            // https://csharpindepth.com/Articles/Singleton
            // https://csharpindepth.com/articles/BeforeFieldInit
        }
        private HtmlData()
        {
            // this is a singleton class
        }

        // so the HTML from the Website browser will be sent to this variable
        // from here the orignial case Banana program will get the HTML data from here and parse it

        private static readonly HtmlData instance = new HtmlData();
        public static HtmlData HtmlDataInstance
        {
            get
            {
                return instance;
            }
        }
        private string htmlString = "";

        public event EventHandler<string> HtmlRetrieved;
        
        public void UpdateHtml(string newHtml)
        {
            htmlString = newHtml;

            OnHtmlUpdated(htmlString);
        }

        public void OnHtmlUpdated(string updatedHtml)
        {
            HtmlRetrieved?.Invoke(new HtmlData(), updatedHtml);
        }
    }
}
