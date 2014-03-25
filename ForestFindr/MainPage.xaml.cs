using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Security;
using System.IO;
using System.Net.Browser;

namespace ForestFindr
{
    public partial class MainPage : UserControl
    {
      
        public MainPage()
        {
            InitializeComponent();
            backgroundsound.Play();
            this.SizeChanged += new SizeChangedEventHandler(MainPage_SizeChanged);
          
        }

        void MainPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width < 1000)
            {
                Logo.Margin = new Thickness(20, 40, 0, 0);
                ContentBorder.Margin = new Thickness(110, 98, 110, 0);
            }
            else
            {
                Logo.Margin = new Thickness(70, 40, 0, 0);
                ContentBorder.Margin = new Thickness(160, 98, 160, 0);
            }
        }
      

        // After the Frame navigates, ensure the HyperlinkButton representing the current page is selected
        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            VisualStateManager.GoToState(this, "Default", true);
            if (e.Content != null)
            {
                if (e.Content.GetType().Equals(typeof(Home)))
                {

                    filterView.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    filterView.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
        }

        // If an error occurs during navigation, show an error window
        private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            e.Handled = true;
            ChildWindow errorWin = new ErrorWindow(e.Uri);
            errorWin.Show();
        }

        //Tim Heuer remeber this...
        //Silverlight 5 wishlist MediaElement MUST have loop option :)
        private void backgroundsound_MediaEnded(object sender, RoutedEventArgs e)
        {
            backgroundsound.Position = new TimeSpan(0);
            backgroundsound.Play();
        }

    }
}