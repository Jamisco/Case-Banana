using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using Operation_Forage_V2;
using SharedHtml;
using Website_Browser;
using Newtonsoft.Json;
using System.Net.Http;

namespace Case_Banana
{
    class Program
    {
        private static Timer atimer = new Timer();
        private static string MainHtml = "";

        static Application app;

        // we need this to open the Website Browser
        [STAThread]
        static void Main(string[] args)
        {
            Uri liquidDetergent = new Uri("https://www.familydollar.com/laundry-care/liquid-detergent-fd");
            Uri costcoBreakfast = new Uri("https://www.costco.com/breakfast.html");

            Uri bing = new Uri("https://www.bing.com/");

            HtmlData.HtmlDataInstance.HtmlRetrieved += HtmlDataInstance_HtmlRetrieved;

            app = new Application();
            app.Run(new MainWindow());

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
               

            parseHtml();
        }

        private static void parseHtml()
        {
            string htmlItemWrapper = "items-wrapper";
            string htmlProduct = "product";

            string htmlTitle = "product-title";
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

            foreach (HtmlParser productHtml in productClass)
            {
                titles.Add(
                    WebUtility.HtmlDecode(productHtml.GetElementByType(htmlTitle, HtmlParser.Type.Class)
                     .GetElementByTagName("span").GetText())
                    );

                prices.Add(productHtml.GetElementByType(htmlPrice, HtmlParser.Type.Class).GetText());

                string imgUrl =
                    productHtml.GetElementByType(htmlProductImage, HtmlParser.Type.Class)
                    .GetElementByTagName("img")
                    .GetElementPropertyValue("data-src");

                // removes all non alphanumeric character,
                // we do this because we save the file using the title and file names cannot have certain characters
                string tempFilePath = imageSavePath + System.Text.RegularExpressions.Regex.Replace(titles.Last(), "[^A-Za-z0-9 -]", "") + ".png";
                //DownloadImageAsync(tempFilePath, imgUrl).Wait();
                DownloadImage(imgUrl, tempFilePath);
                // trying to download image, having weird 403 requiest denied error
                imgPath.Add(tempFilePath);
            }

            StoreData(titles, prices, imgPath);

            // Andddd Done
        }

        private static string imageSavePath = "C:\\Users\\mufuh\\Documents\\Operation Butler\\Family Dollar Detergents Pictures\\";
        private static string dataSavePath = "C:\\Users\\mufuh\\Documents\\Operation Butler\\Mr.Butler\\Test Data\\FD Detergents.json";

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

            System.Drawing.Image image = null;

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
            List<data> _data = new List<data>();

            for (int i = 0; i < titles.Count; i++)
            {
                _data.Add(new data()
                {
                    Title = titles[i],
                    Price = prices[i],
                    ImagePath = imgPaths[i]
                });
            }

            string json = JsonConvert.SerializeObject(_data, Formatting.Indented);
            File.WriteAllText(dataSavePath, json);

        }
       
        class data
        {
            public string Title { get; set; }
            public string Price { get; set; }
            public string ImagePath { get; set; }
        }
    }
}
