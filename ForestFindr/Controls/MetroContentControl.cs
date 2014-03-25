using System.Windows;
using System.Windows.Controls;

namespace ForestFindr.Controls
{
    public class MetroContentControl : ContentControl
    {
        public MetroContentControl()
        {
            this.DefaultStyleKey = typeof(MetroContentControl);

            this.Loaded += MetroContentControl_Loaded;
            this.Unloaded += MetroContentControl_Unloaded;
        }

        private void MetroContentControl_Unloaded(
            object sender, RoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "AfterUnLoaded", false);
        }

        private void MetroContentControl_Loaded(
            object sender, RoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "AfterLoaded", true);
        }
    }
}
