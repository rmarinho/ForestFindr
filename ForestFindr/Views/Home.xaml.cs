using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using ForestFindr.Controls;
using ForestFindr.Helpers;
using ForestFindr.ViewModels;
using GalaSoft.MvvmLight.Messaging;


namespace ForestFindr
{
  
    public partial class Home : Page
    {
        #region Ctor
        public Home()
        {
            InitializeComponent();
            CompositionInitializer.SatisfyImports(this);
            this.Loaded += new System.Windows.RoutedEventHandler(Home_Loaded);
            this.SizeChanged += new System.Windows.SizeChangedEventHandler(Home_SizeChanged);
            Messenger.Default.Register<Messages.ProximityQueryMessage>(this, "ProximityQuery", msg =>
             {
                 load.Visibility = System.Windows.Visibility.Visible;
                 load.Storyboard1.Begin();

             });
        } 
        #endregion
        #region Private Events

        //when the screen is too small loose the margins 
        void Home_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            if (e.NewSize.Width < 1000)
            {
                LayoutRoot.Margin = new Thickness(0, 0, 0, 20);
            }
            else
            {
                LayoutRoot.Margin = new Thickness(0, 0, 0, 100);

            }
        }

        void Home_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            //after we are sure that homeviewmodel already exists let's send them the map instance
            //
            if (this.DataContext != null)
                Messenger.Default.Send<CustomMap>(map);
        }

        // Executes when the user navigates to this page.
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            //clean the view model
            ViewModel.Cleanup();

            base.OnNavigatingFrom(e);
        }
        
        #endregion
        #region ViewModel
        //the since it's a new map also let's create a new viewmodel 
        [Import(RequiredCreationPolicy = CreationPolicy.NonShared)]
        public HomeViewModel ViewModel
        {
            get
            {

                return (DataContext as HomeViewModel);

            }
            set
            {
                DataContext = value;

            }
        }
        
        #endregion
    }
}
