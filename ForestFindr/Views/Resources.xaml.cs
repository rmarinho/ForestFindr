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
using System.Windows.Shapes;
using System.Windows.Navigation;

namespace ForestFindr.Views
{
    public partial class Resources : Page
    {
        public Resources()
        {
            InitializeComponent();
            this.SizeChanged += new SizeChangedEventHandler(Resources_SizeChanged);
        }

        void Resources_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width < 1000)
            {
                LayoutRoot.Margin = new Thickness(0, 0, 0, 20);
            }
            else
            {
                LayoutRoot.Margin = new Thickness(0, 0, 0, 150);
             
            }
        }

        // Executes when the user navigates to this page.
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

    }
}
