using System.Windows;
using System.Windows.Controls;

namespace ForestFindr.Views
{
    public partial class Loading : UserControl
    {
        #region Ctor

        public Loading()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(Loading_Loaded);
            this.Unloaded += new RoutedEventHandler(Loading_Unloaded);
        } 
        #endregion
    
        #region Load/Unload Events

        void Loading_Unloaded(object sender, RoutedEventArgs e)
        {
            Storyboard1.Begin();
        }

        void Loading_Loaded(object sender, RoutedEventArgs e)
        {
            Storyboard1.Stop();
        } 
        #endregion
   
        #region DependencyProperty



        public bool PlayAnimation
        {
            get { return (bool)GetValue(PlayAnimationProperty); }
            set { SetValue(PlayAnimationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PlayAnimation.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PlayAnimationProperty =
            DependencyProperty.Register("PlayAnimation", typeof(bool), typeof(Loading), new PropertyMetadata(false, new PropertyChangedCallback(OnPlayAnimation)));

        private static void OnPlayAnimation(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Loading sh = (Loading)d;
            if (sh.PlayAnimation)
            {
                sh.Visibility = Visibility.Visible;
                sh.Storyboard1.Begin();
            }
            else
            {
                sh.Visibility = Visibility.Collapsed;
                sh.Storyboard1.Stop();
            }
        }
        
        #endregion

    }
}
