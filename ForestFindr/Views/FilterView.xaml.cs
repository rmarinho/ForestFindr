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
using Microsoft.Maps.MapControl;
using System.Globalization;
using GalaSoft.MvvmLight.Messaging;
using ForestFindr.Helpers;
using ForestFindr.Services;
using System.Collections.ObjectModel;
using ForestFindr.RouteService;
using ForestFindr.Web.Model;
using ForestFindr.Controls;
using ForestFindr.Entities;

namespace ForestFindr.Views
{
    public partial class FilterView : UserControl
    {
         private bool startdraw = false;
        private bool draw = false;
        private MapPolyline drawline;
        private MapPolyline drawbuffer;
        private MapPolyline routeLine;
        private MapPolygon drawcir;
        private Location centerpoint;
        private Ellipse center;
        private Location radiusEllipse;
        private double dist;
        private SiteUser currentUser = null;

        private BingPlatformServices bingapi = new BingPlatformServices();

        private bool isLoaded = false;

        public FilterView()
        {
            InitializeComponent();
          
            //Get a Instance of the Map
            Messenger.Default.Register<CustomMap>(this, mp => {
                MapInstance  = mp;
            MapInstance.ViewChangeOnFrame += new EventHandler<MapEventArgs>(MapInstance_ViewChangeOnFrame);
            });

            //After we done of drawing the areas, clear the drawarea 
            Messenger.Default.Register<bool>(this,"draw", bl => {
                ClearDraw();
                ClearFilterChecks();
            });

            //GeoCode is Done, draw and query for areas
            Messenger.Default.Register<Messages.RouteDoneMessage>(this, "RouteDone", msg =>
            {
                DrawRote(msg.Locations, msg.Legs);              
            });

                currentUser = (App.Current.Resources["User"] as SiteUser);
                if (currentUser != null)
                {
                    string locationString  ="";
                    if (!string.IsNullOrEmpty(currentUser.City))
                    {
                    locationString += currentUser.City + ", ";
                    }
                    if (!string.IsNullOrEmpty(currentUser.RegionName))
                    {
                        locationString += currentUser.RegionName + ", ";
                    }

                    if (!string.IsNullOrEmpty(currentUser.PostalCode))
                    {
                        locationString += currentUser.PostalCode + ", ";
                    }

                    if (!string.IsNullOrEmpty(currentUser.Country))
                    {
                        locationString += currentUser.Country;
                     //   filter.Text = currentUser.Country;
                    }
                
                    FromInput.Text = locationString;
                    ToInput.Text = "";
                }
            filtersAccordion.SelectedIndex = 3;
            isLoaded = true;
        }

        void MapInstance_ViewChangeOnFrame(object sender, MapEventArgs e)
        {
            if (drawbuffer != null)
            {
                this.drawbuffer.StrokeThickness = this.BufferSlider.Value * 2000
                         / MiscFunctions.GetMapResolution(this.drawbuffer.Locations[0].Latitude, MapInstance.ZoomLevel);
            }
        }

        /// <remarks>
        ///  map reference used for communicating to parent map control
        /// </remarks>
        public MapLayer _layerDraw;
        public MapLayer _layerRoute;
        private CustomMap _map;
        public CustomMap MapInstance
        {
            get
            {
                return _map;
            }
            set
            {
                _map = value;
                _layerDraw = (MapLayer)_map.FindName("layerDraw");
                _layerRoute = (MapLayer)_map.FindName("layerRoute");
            }
        }

        #region Draw Tools
        /// <summary>
        /// Selection of tool type.
        ///     Proximity
        ///     PolyBuffer
        ///     AOI
        ///     Viewport
        /// </summary>
        private void Draw_RadioButton_Click(object sender, RoutedEventArgs e)
        {
            ClearDraw();
            CheckBox scb = sender as CheckBox;
            if (scb.IsChecked == true)
            {
                ClearFilterChecks();

                switch (scb.Name)
                {
                    case "Proximity":
                        {
                            draw = true;
                            Proximity.IsChecked = true;
                            startdraw = true;
                            _map.MouseLeftButtonUp += new MouseButtonEventHandler(DrawPoint_MouseLeftButtonUp);
                            
                            break;
                        }
                    case "PolyBuffer":
                        {
                            draw = true;
                            PolyBuffer.IsChecked = true;
                            startdraw = true;
                            _map.MouseLeftButtonUp += new MouseButtonEventHandler(DrawPolyline_MouseClick);

                            break;
                        }
                    
                }
            }

        }

        private void MaskPan(object sender, MapMouseEventArgs e)
        {
            //mask map pan events
            e.Handled = true;
        }

        /// <summary>
        /// Proximity setup and teardown
        /// </summary>
        private void DrawPoint_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(draw)
            {
                Location loc = _map.ViewportPointToLocation(e.GetPosition(_map));
                if (startdraw)
                {
                    //first click
                    _layerDraw.Children.Clear();
                    centerpoint = loc;
                    e.Handled = true;
                    _map.MouseMove += new MouseEventHandler(DrawPoint_MouseMove);
                    drawcir = new MapPolygon();
                    drawcir.Stroke = (SolidColorBrush)this.Resources["DrawToolStrokeBrush"];
                    drawcir.Fill = (SolidColorBrush)this.Resources["DrawToolFillBrush"];
                    drawcir.StrokeThickness = 2;
                    drawcir.Locations = DrawCircle(loc, 0.0, 10);
                    _layerDraw.Children.Add(drawcir);

                    center = new Ellipse();
                    center.Name = "Proximity";
                    center.Fill = (SolidColorBrush)this.Resources["DrawToolStrokeBrush"];
                    center.Width = 5;
                    center.Height = 5;
                    MapLayer.SetPositionOrigin(center, PositionOrigin.Center);
                    MapLayer.SetPosition(center, loc);
                    _layerDraw.Children.Add(center);
                    startdraw = false;
                    
                }
                else
                {
                    //second click
                    draw = false;
                    radiusEllipse = loc;
                    _map.MouseMove -= DrawPoint_MouseMove;
                   
                    startdraw = true;
                    //initiate a map ViewChange event
                    //_map.SetView(_map.Center, _map.ZoomLevel);
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
           if(!string.IsNullOrEmpty(FromInput.Text) && !string.IsNullOrEmpty(ToInput.Text))
            bingapi.GetRoute(FromInput.Text, ToInput.Text);
          
        }

        private void ToInput_GotFocus(object sender, RoutedEventArgs e)
        {
            ToInput.SelectAll();
        }

        private void FromInput_GotFocus(object sender, RoutedEventArgs e)
        {
            FromInput.SelectAll();
        }

        private void Input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!string.IsNullOrEmpty(FromInput.Text) && !string.IsNullOrEmpty(ToInput.Text))
                bingapi.GetRoute(FromInput.Text, ToInput.Text);
            }
        }

     
        /// <summary>
        /// Dynamic stretch for proximity draw tool
        /// </summary>
        private void DrawPoint_MouseMove(object sender, MouseEventArgs e)
        {
                Location p = _map.ViewportPointToLocation(e.GetPosition(_map));
                double dx = (p.Longitude - centerpoint.Longitude);
                double dy = (p.Latitude - centerpoint.Latitude);
                dist = Math.Sqrt(dx * dx + dy * dy);
                drawcir.Locations = DrawCircle(centerpoint, dist, 10);
        }

        /// <summary>
        /// DrawCircle
        ///     function to draw a circle 
        /// </summary>
        /// <param name="cp">Center point Location lat,lon</param>
        /// <param name="radius">double radius in decimal degrees</param>
        /// <param name="step">int step to indicate the increment to step around 360 degree circle</param>
        /// <returns></returns>
        private LocationCollection DrawCircle(Location cp, double radius, int step)
        {
            LocationCollection locs = new LocationCollection();
            drawcir.Locations = new LocationCollection();
            for (float angle = 0; angle <= 360; angle += step)
            {
                Location pt = new Location();
                pt.Longitude = (radius * Math.Cos(angle * 0.0174532925)) + cp.Longitude;
                pt.Latitude = (radius * Math.Sin(angle * 0.0174532925)) + cp.Latitude;
                locs.Add(pt);
            }
            return locs;
        }

        /// <summary>
        /// DrawPolyline_MouseClick
        ///     draw polyline as a series of click nodes
        ///     drawn as two polylines
        ///         base buffer thickness
        ///         top polyline
        /// </summary>
        private void DrawPolyline_MouseClick(object sender, MouseButtonEventArgs e)
        {
            if(draw)
            {
                Location loc = _map.ViewportPointToLocation(e.GetPosition(_map));
                if (startdraw)
                {
                    //first click
                    _layerDraw.Children.Clear();
                    //add a profile drawline of zero length
                    _layerDraw.CaptureMouse();
                    _map.MouseMove += new MouseEventHandler(DrawPolyline_MouseMove);
                    _map.MouseDoubleClick += new EventHandler<MapMouseEventArgs>(DrawPolyline_MouseDoubleClick);

                    drawbuffer = new MapPolyline();
                    drawbuffer.Name = "Buffer";
                    drawbuffer.StrokeEndLineCap = PenLineCap.Round;
                    drawbuffer.StrokeStartLineCap = PenLineCap.Round;
                    drawbuffer.StrokeLineJoin = PenLineJoin.Round;
                    drawbuffer.StrokeMiterLimit = 0;
                    drawbuffer.Stroke = (SolidColorBrush)this.Resources["DrawToolFillBrush"];

                    drawbuffer.Locations = new LocationCollection();
                    drawbuffer.Locations.Add(loc);
                    drawbuffer.Locations.Add(loc);
                    _layerDraw.Children.Add(drawbuffer);

                    drawline = new MapPolyline();
                    drawline.Stroke = (SolidColorBrush)this.Resources["DrawToolStrokeBrush"];
                    drawline.StrokeThickness = 2;
                    drawline.StrokeEndLineCap = PenLineCap.Flat;
                    drawline.StrokeLineJoin = PenLineJoin.Bevel;
                    drawline.StrokeMiterLimit = 0;
                    drawline.Locations = new LocationCollection();
                    drawline.Locations.Add(loc);
                    drawline.Locations.Add(loc);
                    _layerDraw.Children.Add(drawline);
                    startdraw = false;
                }
                else
                {
                    //subsequent clicks
                    drawline.Locations.Add(loc);
                    drawbuffer.Locations.Add(loc);
                    if (drawbuffer.Locations.Count < 4)
                    {
                        drawbuffer.StrokeThickness = BufferSlider.Value * 2000 / MiscFunctions.GetMapResolution(loc.Latitude, _map.ZoomLevel);
                    }
                }
           }
 
        }

        private void DrawRote(ObservableCollection<Location> points, ObservableCollection<RouteLeg> Legs)
        {
            _layerRoute.Children.Clear();
            _layerDraw.Children.Clear();
            Color routeColor = Colors.Blue;
            SolidColorBrush routeBrush = new SolidColorBrush(routeColor);
     
            drawbuffer = new MapPolyline();
            drawbuffer.Name = "Buffer";
            drawbuffer.StrokeEndLineCap = PenLineCap.Round;
            drawbuffer.StrokeStartLineCap = PenLineCap.Round;
            drawbuffer.StrokeLineJoin = PenLineJoin.Round;
            drawbuffer.StrokeMiterLimit = 0;
            drawbuffer.Stroke = (SolidColorBrush)this.Resources["DrawToolFillBrush"];

            drawbuffer.StrokeThickness = BufferSlider.Value * 2000 / MiscFunctions.GetMapResolution(points[1].Latitude, _map.ZoomLevel);
            routeLine = new MapPolyline();
            routeLine.Locations = new LocationCollection();
            routeLine.Stroke = routeBrush;
            routeLine.Opacity = 0.65;
            routeLine.StrokeThickness = 5.0;
            for (int i = 0; i < points.Count; i=i+5)
            {
                if (drawbuffer.Locations == null)
                    drawbuffer.Locations = new LocationCollection();
                drawbuffer.Locations.Add(points[i]);
                routeLine.Locations.Add(points[i]);
              
            }
           
            _layerRoute.Children.Add(routeLine);
            _layerDraw.Children.Add(drawbuffer);
            LocationRect rect = new LocationRect(routeLine.Locations[0], routeLine.Locations[routeLine.Locations.Count - 1]);

            foreach (ItineraryItem itineraryItem in Legs[0].Itinerary)
            {
                Ellipse point = new Ellipse();
                point.Width = 10;
                point.Height = 10;
                point.Fill = new SolidColorBrush(Colors.Red);
                point.Opacity = 0.65;
                Location location = new Location(itineraryItem.Location.Latitude, itineraryItem.Location.Longitude);
                MapLayer.SetPosition(point, location);
                MapLayer.SetPositionOrigin(point, PositionOrigin.Center);
                point.Tag = itineraryItem;

                _layerRoute.Children.Add(point);
            }


          
           
            
           
        }

        /// <summary>
        /// Polyline rubberband line segment stretch
        /// </summary>
        private void DrawPolyline_MouseMove(object sender, MouseEventArgs e)
        {
                if (!startdraw)
                {
                    Location loc = _map.ViewportPointToLocation(e.GetPosition(_map));
                    drawbuffer.Locations[drawbuffer.Locations.Count - 1] = loc;
                    drawline.Locations[drawline.Locations.Count - 1] = loc;
                }
        }

        /// <summary>
        /// Double click finishes buffer polyline draw
        /// </summary>
        private void DrawPolyline_MouseDoubleClick(object sender, MapMouseEventArgs e)
        {
                if (!startdraw)
                {
                    draw = false;
                    e.Handled = true;
                    Location loc = _map.ViewportPointToLocation(e.ViewportPoint);
                    drawbuffer.Locations[drawbuffer.Locations.Count - 1] = loc;
                    drawline.Locations[drawline.Locations.Count - 1] = loc;
                    startdraw = true;
                 
                }
        }

        /// <summary>
        /// Slider changes buffer radius StrokeThickness approximated using GetMapResolution
        /// </summary>
        private void BufferSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isLoaded)
            {
                BufferText.Text = string.Format("{0:F1}", e.NewValue);
                BufferText1.Text = string.Format("{0:F1}", e.NewValue);
                if (drawbuffer != null)
                {
                    draw = false;
                    drawbuffer.StrokeThickness =  Rangedetour   * 2000 / MiscFunctions.GetMapResolution(drawbuffer.Locations[0].Latitude, _map.ZoomLevel);
                    //ClearFilterChecks();   
                }
            }
        }

   
        /// <summary>
        /// Cleanup Draw tools
        /// </summary>
        private void ClearDraw()
        {
            if (_layerDraw != null)
            {
                startdraw = false;
                _layerDraw.Children.Clear();
                _layerRoute.Children.Clear();
                _map.MouseLeftButtonUp -= DrawPoint_MouseLeftButtonUp;
                _map.MouseLeftButtonUp -= DrawPolyline_MouseClick;
                _map.MouseMove -= DrawPoint_MouseMove;
                _map.MouseMove -= DrawPolyline_MouseMove;
                _map.MouseDoubleClick -= DrawPolyline_MouseDoubleClick;
            }
   
        }               

        /// <summary>
        /// Clear checkboxes in draw tools selection
        /// </summary>
        public void ClearFilterChecks()
        {
            if (Proximity != null)
            {
                Proximity.IsChecked = false;
                PolyBuffer.IsChecked = false;
            }
        }

        #endregion

        private void filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null && e.AddedItems.Count > 0)
            {
                Selectedcountry = (Country)e.AddedItems[0];
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

            switch (this.filtersAccordion.SelectedIndex)
            {
                case 0:
                    if (routeLine != null)
                    {
                        Messenger.Default.Send<Messages.PolyBufferQueryMessage>(new Messages.PolyBufferQueryMessage
                        {
                            Locations = routeLine.Locations,
                            Buffer = Rangedetour * 1000
                        }
                      , "PolyLineQuery");
                    }
                    break;
                case 1:
                    if (centerpoint != null && radiusEllipse != null)
                    {
                        LocationCollection col = new LocationCollection();
                        col.Add(MapLayer.GetPosition(center));
                        Messenger.Default.Send<Messages.ProximityQueryMessage>(new Messages.ProximityQueryMessage
                        {
                            Locations = col,
                            Radius = MiscFunctions.HaversineDistance(centerpoint, radiusEllipse)
                        }
                                   , "ProximityQuery");
                    }
                    break;
                case 2:
                    if (drawline != null)
                    {
                        Messenger.Default.Send<Messages.PolyBufferQueryMessage>(new Messages.PolyBufferQueryMessage
                        {
                            Locations = drawline.Locations,
                            Buffer = Rangedetour * 1000
                        }
                     , "PolyLineQuery");
                    }
                    break;
                case 3:
                    if (Selectedcountry != null)
                    {
                        Messenger.Default.Send<Messages.CountryQueryMessage>(new Messages.CountryQueryMessage
                        {
                            SelectedCountry = Selectedcountry
                        });
                    }
                    else
                        MessageBox.Show("Must select country");
                    break;
                default:
                    break;
            }
       
        }

        private void Accordion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            ClearDraw();

            if (e.AddedItems.Count > 0)
            {
                switch ((((object[])(e.AddedItems))[0] as AccordionItem).Name)
                {

                    case "acCountry":
                        {
                            break;
                        }
                    case "acRoute":
                        {
                            break;
                        }
                    case "acProximity":
                        {
                           
                            draw = true;
                            Proximity.IsChecked = true;
                            startdraw = true;
                            _map.MouseLeftButtonUp += new MouseButtonEventHandler(DrawPoint_MouseLeftButtonUp);

                            break;
                        }
                    case "acLine":
                        {
                            draw = true;
                            PolyBuffer.IsChecked = true;
                            startdraw = true;
                            _map.MouseLeftButtonUp += new MouseButtonEventHandler(DrawPolyline_MouseClick);

                            break;
                        }

                }
            }
        }




        public Country Selectedcountry
        {
            get { return (Country)GetValue(SelectedcountryProperty); }
            set { SetValue(SelectedcountryProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Selectedcountry.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedcountryProperty =
            DependencyProperty.Register("Selectedcountry", typeof(Country), typeof(FilterView), new PropertyMetadata(null));




        public double Rangedetour
        {
            get { return (double)GetValue(RangedetourProperty); }
            set { SetValue(RangedetourProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Rangedetour.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RangedetourProperty =
            DependencyProperty.Register("Rangedetour", typeof(double), typeof(FilterView), new PropertyMetadata(50.0));

        
        
    }
}
