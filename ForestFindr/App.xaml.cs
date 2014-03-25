using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Xml.Linq;
using ForestFindr.Entities;
using System.Threading;

namespace ForestFindr
{
    public partial class App : Application
    {
        public App()
        {
            this.Startup += this.Application_Startup;
            this.UnhandledException += this.Application_UnhandledException;

            InitializeComponent();
        }

        private const string ApiUrl = "http://api.ipinfodb.com/v2/ip_query.php?key={0}&ip={1}&timezone=false";

        private const string ApiKey = "470b27ce05cd7a2db867395f478fb49a8e93b479dca7a45ad59935250f3d9053";
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");
       

            string ipAddress = e.InitParams["IPAddress"];
        //localhost testing
            if(ipAddress.Equals("::1"))
            ipAddress = "89.152.111.225";
            string reqUrl = string.Format(ApiUrl, ApiKey, ipAddress);
            WebClient client = new WebClient();
            client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(client_DownloadStringCompleted);
            client.DownloadStringAsync(new Uri(reqUrl),new object());

          
        }

        public void client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            (sender as WebClient).DownloadStringCompleted -= client_DownloadStringCompleted;
            if (e.Error == null)
            {
                StringReader sr = new StringReader(e.Result);
                XElement respElement = XElement.Load(sr);
                string callStatus = (string)respElement.Element("Status");
                if (callStatus.Equals("OK"))
                {
                    SiteUser user = new SiteUser()
                    {
                        IP = (string)respElement.Element("Ip"),
                        City = (string)respElement.Element("City"),
                        Country = (string)respElement.Element("CountryName"),
                        CountryCode = (string)respElement.Element("CountryCode"),
                        RegionCode = (string)respElement.Element("RegionCode"),
                        RegionName = (string)respElement.Element("RegionName"),
                        PostalCode = (string)respElement.Element("ZipPostalCode"),
                        Latitude = (double)respElement.Element("Latitude"),
                        Longitude = (double)respElement.Element("Longitude")
                    };
                    this.Resources.Add("User", user);

                }
            }
            this.RootVisual = new MainPage();
        }

        public SiteUser CurrentUser;
    

        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            // If the app is running outside of the debugger then report the exception using
            // a ChildWindow control.
            if (!System.Diagnostics.Debugger.IsAttached)
            {
                // NOTE: This will allow the application to continue running after an exception has been thrown
                // but not handled. 
                // For production applications this error handling should be replaced with something that will 
                // report the error to the website and stop the application.
                e.Handled = true;
                ChildWindow errorWin = new ErrorWindow(e.ExceptionObject);
                errorWin.Show();
            }
        }
    }
}