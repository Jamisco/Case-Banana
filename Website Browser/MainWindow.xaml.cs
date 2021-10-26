using System;
using System.Diagnostics;
using System.Timers;
using System.Windows;
using CefSharp.Wpf;
using CefSharp;
using SharedHtml;

namespace Website_Browser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Uri liquidDetergent = new Uri("https://www.familydollar.com/laundry-care/liquid-detergent-fd");
        Uri costcoBreakfast = new Uri("https://www.costco.com/breakfast.html");

        Uri bing = new Uri("https://www.bing.com/");

        Timer mainTimer = new Timer();

        bool browserInit = false;

        public ChromiumWebBrowser cefBrowser;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            mainTimer.Interval = 5000;
            mainTimer.Elapsed += MainTimer_Elapsed;
            mainTimer.Start();

            CefSettings settings = new CefSettings();
            Cef.Initialize(settings);

            cefBrowser = new ChromiumWebBrowser(liquidDetergent.OriginalString);

            cefBrowser.RenderSize = RenderSize;

            mainGrid.Children.Add(cefBrowser);

        }

        private void MainTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // the timer is used so we can extract html after x seconds, for some reason cef browser initialized events are not working
            // we know the browser took max 10 seconds to load the website, so for performance we got with 5

            string html = "";

            if (browserInit == false)
            {
                cefBrowser.GetSourceAsync().ContinueWith(taskHtml =>
                {
                    browserInit = true;

                    html = taskHtml.Result;

                }).Wait();
            }

            if(browserInit == true)
            {
                Dispatcher.Invoke(() =>
                {
                    browserInit = true;

                    mainTimer.Stop(); // we got what we came for

                    Cef.Shutdown();
                });

                HtmlData.HtmlDataInstance.UpdateHtml(html);
            }     
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                Cef.Shutdown();
            }
            catch (Exception)
            {
                // whatever
            }
           
        }
    }
}
