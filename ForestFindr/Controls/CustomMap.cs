using System.Collections.ObjectModel;
using System.Windows;
using Microsoft.Maps.MapControl;

namespace ForestFindr.Controls
{
    public class CustomMap : Map
    {

        public CustomMap()
        {
           
        }


        #region MapZoomLevelProperty



        // Map Zoom level property
        public static readonly DependencyProperty MapZoomLevelProperty =
            DependencyProperty.Register("MapZoomLevel",
            typeof(double), typeof(CustomMap),
            new PropertyMetadata(
                new PropertyChangedCallback(OnMapZoomLevelChanged)));

        public double MapZoomLevel
        {
            get { return (double)GetValue(MapZoomLevelProperty); }
            set { SetValue(MapZoomLevelProperty, value); }
        }


     
        private static void OnMapZoomLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            double z = (double)((Map)d).ZoomLevel;
            z = (double)e.NewValue;
            ((Map)d).ZoomLevel = z;
        }
        #endregion

        #region MapCenterLatitudeProperty
        // MapCenter Position Latitude Property
        public static readonly DependencyProperty MapCenterPositionLatitudeProperty =
            DependencyProperty.Register("MapCenterPositionLatitude",
            typeof(double), typeof(CustomMap),
            new PropertyMetadata(
                new PropertyChangedCallback(OnMapCenterPositionLatitudeChanged)));


        public double MapCenterPositionLatitude
        {
            get { return (double)GetValue(MapCenterPositionLatitudeProperty); }
            set { SetValue(MapCenterPositionLatitudeProperty, value); }
        }

        private static void OnMapCenterPositionLatitudeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Location l = (Location)((Map)d).GetValue(Map.CenterProperty);
            l.Latitude = (double)e.NewValue;
            double z = (double)((Map)d).ZoomLevel;
            ((Map)d).SetView(l, z);
        }
        #endregion

        #region MapCenterLongitudeProperty
        // MapCenter Position Latitude Property
        public static readonly DependencyProperty MapCenterPositionLongitudeProperty =
            DependencyProperty.Register("MapCenterPositionLongitude",
            typeof(double), typeof(CustomMap),
            new PropertyMetadata(
                new PropertyChangedCallback(OnMapCenterPositionLongitudeChanged)));

        public double MapCenterPositionLongitude
        {
            get { return (double)GetValue(MapCenterPositionLongitudeProperty); }
            set { SetValue(MapCenterPositionLongitudeProperty, value); }
        }

        private static void OnMapCenterPositionLongitudeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Location l = (Location)((Map)d).GetValue(Map.CenterProperty);
            l.Longitude = (double)e.NewValue;
            double z = (double)((Map)d).ZoomLevel;
            ((Map)d).SetView(l, z);
        }
        #endregion

        #region MapCenterPositionProperty
        // Map Center Position Property as a Point
        public static readonly DependencyProperty MapCenterPositionProperty =
            DependencyProperty.Register("MapCenterPosition",
            typeof(Point), typeof(CustomMap),
            new PropertyMetadata(
                new PropertyChangedCallback(OnMapCenterPositionChanged)));

        public Point MapCenterPosition
        {
            get { return (Point)GetValue(MapCenterPositionProperty); }
            set { SetValue(MapCenterPositionProperty, value); }
        }

        private static void OnMapCenterPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Location l = (Location)((Map)d).GetValue(Map.CenterProperty);
            Point p = new Point(l.Latitude, l.Longitude);
            p = (Point)e.NewValue;
            double z = (double)((Map)d).ZoomLevel;
            ((Map)d).SetView(new Location(p.X, p.Y), z);
        }
        #endregion

        #region MapLayersProperty

        public ObservableCollection<MapLayer> MapLayers
        {
            get { return (ObservableCollection<MapLayer>)GetValue(MapLayersProperty); }
            set
            {
                SetValue(MapLayersProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for MapLayers.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MapLayersProperty =
            DependencyProperty.Register("MapLayers", typeof(ObservableCollection<MapLayer>), typeof(CustomMap), new PropertyMetadata(OnMapLayersChanged));


        private static void OnMapLayersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
           
            if ((e.NewValue as ObservableCollection<MapLayer>).Count > 0 &&  ((Map)d).Children.Count == 0)
            {
                foreach (var item in (e.NewValue as ObservableCollection<MapLayer>))
                {
                    
                    ((Map)d).Children.Add(item);
                }

            }
            else
            {
                ((Map)d).Children.Clear();
            }

        }
        
        #endregion
      
    }
}
