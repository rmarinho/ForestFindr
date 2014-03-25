using System;
using System.Windows.Media;
using Microsoft.Maps.MapControl;


namespace ForestFindr
{
    public partial class Pin
    {
        public Pin()
        {
            InitializeComponent();
        }

        private Map _map;
        public Map MapInstance {
            get
            {
                return _map;
            } 
            set 
            {
                _map = value;
                _map.ViewChangeOnFrame += MapViewChangeOnFrame;
                ApplyPowerLawScaling(_map.ZoomLevel);
            }
        }

        private bool _showCustomPin;
        public bool ShowCustomPin
        {
            get { return _showCustomPin; }
            set
            {
                _showCustomPin = value;
                marker.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private bool _showCustomTree;
        public bool ShowCustomTree
        {
            get { return _showCustomTree; }
            set
            {
                _showCustomTree = value;
                markerTree.Visibility = System.Windows.Visibility.Visible;
            }
        }

        public ImageSource ImageSource
        {
            get { return PinImage.Source; }
            set { PinImage.Source = value; 
		
			}
        }

        void MapViewChangeOnFrame(object sender, MapEventArgs e)
        {
            ApplyPowerLawScaling(MapInstance.ZoomLevel);
        }

        private void ApplyPowerLawScaling(double currentZoomLevel)
        {
            double scale = Math.Pow(0.05*(currentZoomLevel + 1), 2) + 0.01;
            if (scale > 1) scale = 1;
            if (scale < 0.125) scale = 0.125;
            PinScaleTransform.ScaleX = scale;
            PinScaleTransform.ScaleY = scale;
         
        }
    }
}
