using Operation_Forage_V2;
using SharedHtml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using Website_Browser;

namespace Case_Banana
{
    class Program
    {
        private static string MainHtml = "";

        static Application app;
        static Task beginParse;

        // we need this to open the Website Browser
        [STAThread]
        static void Main()
        {
            Uri liquidDetergent = new Uri("https://www.familydollar.com/laundry-care/liquid-detergent-fd");
            Uri costcoBreakfast = new Uri("https://www.costco.com/breakfast.html");

            Uri bing = new Uri("https://www.bing.com/");

            HtmlData.HtmlDataInstance.HtmlRetrieved += HtmlDataInstance_HtmlRetrieved;

            app = new Application();
            app.Run(new MainWindow());

            // wait for the parse method to finish, that we dont alert the user that the program is finished prematurely
            beginParse.Wait();

            Console.WriteLine("\n\n\n" + "We are Done Here...");
            Console.ReadLine();
        }

        private static void HtmlDataInstance_HtmlRetrieved(object sender, string e)
        {
            MainHtml = e;

            app.Dispatcher.Invoke(() =>
            {
                // we have no more use for the application once the html has been retrieved
                app.Shutdown();
            });

            beginParse = Task.Factory.StartNew(() => ParseHtml());
        }

        private static void ParseHtml()
        {
            string htmlItemWrapper = "items-wrapper";
            string htmlProduct = "product";

            //string htmlTitle = "product-title";
            string htmlProductImage = "product-image";
            string htmlPrice = "list-price-text";

            List<string> titles = new List<string>();
            List<string> prices = new List<string>();
            List<string> imgPath = new List<string>();

            HtmlParser htmlParser = new HtmlParser(MainHtml, true, true, true);

            HtmlParser itemWrappers = htmlParser.GetElementByType(htmlItemWrapper, HtmlParser.Type.Class);

            List<HtmlParser> productClass = itemWrappers.GetElementsByType(htmlProduct, HtmlParser.Type.Class);

            // this is the path where the folder will be saved
            Directory.CreateDirectory(imageSavePath);
            DirectoryInfo di = new DirectoryInfo(imageSavePath);

            // clear all files in directory bcuz we run this multiple times and want a fresh start every time
            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }

            int index = 0;

            foreach (HtmlParser productHtml in productClass)
            {
                // Do not add Spaces in Path, makes life much more difficult
                titles.Add("Picture" + index);
                index++;

                string fileName = titles.Last() + ".png";

                prices.Add(productHtml.GetElementByType(htmlPrice, HtmlParser.Type.Class).GetText());

                string imgUrl =
                    productHtml.GetElementByType(htmlProductImage, HtmlParser.Type.Class)
                    .GetElementByTagName("img")
                    .GetElementPropertyValue("data-src");

                string tempFilePath = imageSavePath + fileName;
                DownloadImage(imgUrl, tempFilePath);
    
                // since we are creating a js file, we will be using the local path
                imgPath.Add("./Images/" + fileName);
            }

             StoreData(titles, prices, imgPath);

            // Andddd Done
        }

        private static readonly string dataSavePath = @"C:\Users\mufuh\Documents\Operation Butler\Mr.Butler\Test-Data\FD-DetergentInfo.js";
        private static readonly string imageSavePath = @"C:\Users\mufuh\Documents\Operation Butler\Mr.Butler\Test-Data\Images\";

        // Code taken from here - https://codesnippets.fesslersoft.de/how-to-download-a-image-from-url-in-c-and-vb-net/
        private static void DownloadImage(string url, string saveFilename)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            // we need the user agent and default credentials if not,
            //  we get a forbidden request 303 error, which pretty much means the server thinks we are a bot -- which we are.... hehehehehehe
            httpWebRequest.UserAgent = "Case Banana";
            httpWebRequest.UseDefaultCredentials = true;

            var httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            if ((httpWebResponse.StatusCode != HttpStatusCode.OK &&
                httpWebResponse.StatusCode != HttpStatusCode.Moved &&
                httpWebResponse.StatusCode != HttpStatusCode.Redirect)
                || !httpWebResponse.ContentType.StartsWith("image", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            using (var stream = httpWebResponse.GetResponseStream())
            {
                using (var fileStream = File.OpenWrite(saveFilename))
                {
                    var bytes = new byte[4096];
                    var read = 0;
                    do
                    {
                        if (stream == null)
                        {
                            continue;
                        }
                        read = stream.Read(bytes, 0, bytes.Length);
                        fileStream.Write(bytes, 0, read);
                    } while (read != 0);
                }
            }
        }
        private static void StoreData(List<string> titles, List<string> prices, List<string> imgPaths)
        {
            List<Data> _data = new List<Data>();

            for (int i = 0; i < titles.Count; i++)
            {
                _data.Add(new Data()
                {
                    Title = titles[i],
                    Price = prices[i],                  
                    ImagePath = imgPaths[i]
                });
            }

            string objectTitle = "DetergentInfo";

            string JSObject = $"const {objectTitle} = [\n";

            foreach (Data props in _data)
            {
                JSObject += props.CreateJSObject();
            }

            JSObject += $"];\n\nexport default {objectTitle};";

            File.WriteAllText(dataSavePath, JSObject);
        }

        private static string Indent(int count)
        {
            return new string(' ', count);
        }

        class Data
        {
            public string Title { get; set; }
            public string Price { get; set; }
            public string ImagePath { get; set; }
            public string CreateJSObject()
            {
                string JSObject = "";

                JSObject +=
                    Indent(2) + "{\n" +
                     Indent(4) + $"Title: '{Title}',\n" +
                     Indent(4) + $"Price: '{Price}',\n" +
                     Indent(4) + $"Image: require('{ImagePath}'),\n" +
                   Indent(2) + "},\n";

                return JSObject;
            }
        }
    }
}
