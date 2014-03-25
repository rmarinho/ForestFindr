using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using ForestFindr.Controls;
using ForestFindr.Entities;
using ForestFindr.Helpers;
using ForestFindr.Services;
using ForestFindr.XAMLayersService;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Maps.MapControl;


namespace ForestFindr.ViewModels
{
    /// <summary>
    /// This class contains properties that a HOmeView can data bind to.
    /// <para>
    /// It receives messages from the filter view to call services and draw the data return on the layer
    /// </para>
    /// We use MEF to compose the view and the viewmodel
    /// </summary>
    /// 
    [Export]
    public class HomeViewModel : ViewModelBase
    {
        #region Fields
        private int recordLimit = 1000;
        private Dictionary<string, LoadStyle> LayerStyle = new Dictionary<string, LoadStyle>();
        private string area = null;
        private CustomMap currentMap = null;
        ChildWindow cw;
        PhotoServices photoservices;
        #endregion

        #region Ctor
        /// <summary>
        /// Initializes a new instance of the HomeViewModel class.
        /// </summary>
        public HomeViewModel()
        {
            InitializeProperties();
            RegisterMessengerMessages();
            //HACK: Force to be this culture for ',' split to work 
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");
       
        }

        private void InitializeProperties()
        {

            Layers = new ObservableCollection<MapLayer>();
            photoservices = new PhotoServices();
            LayerStyle.Add("layerAreas", new LoadStyle("#FFFF0000", "#FF00FF00", 0.25));
            Layers.Add(new MapLayer { Name = "layerAreas" });
            Layers.Add(new MapLayer { Name = "layerDraw" });
            Layers.Add(new MapLayer { Name = "layerpanoramio" });
            Layers.Add(new MapLayer { Name = "layerRoute" });
            CurrentUser = (App.Current.Resources["User"] as SiteUser);
            if (CurrentUser != null)
            {
                CurrentCenter = new Point(CurrentUser.Latitude, CurrentUser.Longitude);
                LocationCollection coll = new LocationCollection();
                coll.Add(new Location(CurrentUser.Latitude, CurrentUser.Longitude));
                CurrentZoomLevel = 8;
                makeQuery("buffer", "Proximity", "areas",coll, 100000);
            }
        }

        private void RegisterMessengerMessages()
        {
            //We need the map instace because some stuff doesn't support binding and we need some map events to resize pins
            Messenger.Default.Register<CustomMap>(this, mp =>
            {
                currentMap = mp;
                
            });

            //The Filter views sends a query
            Messenger.Default.Register<Messages.ProximityQueryMessage>(this, "ProximityQuery", msg =>
            {
                if(VisualStateName == "PhotoState")
                VisualStateName = "DefaultState";
                makeQuery("buffer", "Proximity", "areas", msg.Locations, msg.Radius);
            });

            //The Filter views sends another query
            Messenger.Default.Register<Messages.PolyBufferQueryMessage>(this, "PolyLineQuery", msg =>
            {
                if (VisualStateName == "PhotoState")
                    VisualStateName = "DefaultState";
                makeQuery("buffer", "Buffer", "areas", msg.Locations, msg.Buffer);
            });
            Messenger.Default.Register<Messages.CountryQueryMessage>(this, msg =>
            {
                if (VisualStateName == "PhotoState")
                    VisualStateName = "DefaultState";
                makeQueryCountry("name", "Name", "areas", msg.SelectedCountry.name_iso);
            });
        }

      
        #endregion

        #region Get Data


        private void makeQueryCountry(string querytype, string drawtype, string table, string country)
        {
            IsBusy = true;
            double reduce = 5000 - CurrentZoomLevel * 500;
            SpatialServices.XAMLService.GetSQLDataXAMLCompleted += new EventHandler<GetSQLDataXAMLCompletedEventArgs>(XAMLService_GetSQLDataXAMLCompleted);
            XAMLParameters parameters = new XAMLParameters();
            parameters.table = table;
            parameters.reduce = reduce;
            parameters.querytype = querytype;
            parameters.points = country;
            SpatialServices.XAMLService.GetSQLDataXAMLAsync(parameters, new object());
        
        }

        /// <summary>
        /// let's query our Xaml Service, we need the querytype, the draw type is just to  chen the number of points
        /// we also can say what table we are querying 
        /// finally we pass the locations and the distance that the buffer has.
        /// </summary>
        /// 

        private void makeQuery(string querytype, string drawtype, string table, LocationCollection points, double radius)
        {
            IsBusy = true;
            double reduce = 5000 - CurrentZoomLevel * 500;
            switch (drawtype)
            {
                case "Proximity":
                    {
                        if (points[0] != null)
                            area = points[0].Longitude + " " + points[0].Latitude;
                    }
                    break;
                case "Buffer":
                    {
                        if (points[0] != null)
                        {
                            StringBuilder sb = new StringBuilder();
                            bool first = true;
                            foreach (Location l in points)
                            {
                                if (!first) sb.Append(",");
                                sb.Append(l.Longitude + " " + l.Latitude);
                                first = false;
                            }
                            area = sb.ToString();
                        }
                    }
                    break;
                default:
                    break;
            }


            if (area != null)
            {
                //have to show load animation
                SpatialServices.XAMLService.GetSQLDataXAMLCompleted += new EventHandler<GetSQLDataXAMLCompletedEventArgs>(XAMLService_GetSQLDataXAMLCompleted);
                XAMLParameters parameters = new XAMLParameters();
                parameters.table = table;
                parameters.querytype = querytype;
                parameters.reduce = reduce;
                parameters.radius = radius;
                parameters.points = area;
                SpatialServices.XAMLService.GetSQLDataXAMLAsync(parameters, new object());
            }

        }

        void XAMLService_GetSQLDataXAMLCompleted(object sender, GetSQLDataXAMLCompletedEventArgs e)
        {
            IsBusy = false;
            SpatialServices.XAMLService.GetSQLDataXAMLCompleted -= (XAMLService_GetSQLDataXAMLCompleted);
            if (e.Error == null)
                DrawAreas(e.Result);
        } 
        #endregion

        #region Draw Areas
        /// <summary>
        /// for each of the area return let's draw it on the area layer
        /// also let's add some ui events to tweak opaciy of layer and add tooltip
        /// </summary>
        /// 
        LocationRect CenterRectAreas;
        LocationCollection AllPointsInLayer;
        private void DrawAreas(XAMLResponse e)
        {
            CenterRectAreas = new LocationRect();
            AllPointsInLayer = new LocationCollection();
            int resultCnt = e.OutputFields.Count;
            if (resultCnt > 0)
            {
                if (resultCnt < recordLimit)
                {
                    if (Layers.Count > 0)
                    {
                        MapLayer currentLayer = Layers[0];
                        MapLayer newLayer = (MapLayer)XamlReader.Load(e.XAML);
                        currentLayer.Children.Clear();
                        Layers[2].Children.Clear();

                        currentLayer.Children.Add(newLayer);

                        foreach (XAMLFields shp in e.OutputFields)
                        {
                            UIElement el = (UIElement)newLayer.FindName(shp.ID);

                            if (el != null)
                            {
                                el.MouseLeftButtonUp += new MouseButtonEventHandler(el_MouseLeftButtonUp);
                                el.MouseEnter += polygon_MouseEnter;
                                el.MouseLeave += polygon_MouseLeave;

                                Area newArea = new Area();

                                newArea.Name = shp.Fields["name"];
                                newArea.Country = shp.Fields["country"].ToString();
                                newArea.DesignationType = shp.Fields["desig_type"];
                                newArea.Designation = shp.Fields["desig_eng"];
                                newArea.RepArea = shp.Fields["rep_area"];

                                int year = 0;
                                int.TryParse(shp.Fields["status_yr"], out year);
                                if (year != 0)
                                    newArea.StatusYear = year;
                                newArea.Wdpaid = int.Parse(shp.Fields["wdpaid"]);
                                newArea.Iucncat = shp.Fields["iucncat"];

                                StringBuilder label = new StringBuilder("\n");
                                label.Append("Name :" + shp.Fields["name"] + "\n");
                                label.Append("Country :" + shp.Fields["country"] + "\n");
                                label.Append("Designation :" + shp.Fields["desig_eng"] + "\n");
                                label.Append("Type :" + shp.Fields["desig_type"] + "\n");
                                label.Append("Total Area :" + shp.Fields["rep_area"] + "\n");
                                label.Append("Iucncat :" + shp.Fields["iucncat"] + "\n");
                                label.Append("Continent :" + shp.Fields["continent"] + "\n");

                                (el as FrameworkElement).Tag = newArea;

                                ToolTip tt = AddToolTip(label.ToString());
                                ToolTipService.SetToolTip(el, tt);


                                if (el.GetType().Equals(typeof(Pushpin)))
                                {
                                    Pushpin p = (Pushpin)el;

                                    AllPointsInLayer.Add(p.Location);

                                    Pin pin = new Pin { ShowCustomPin = true };
                                    pin.MouseLeftButtonUp += new MouseButtonEventHandler(el_MouseLeftButtonUp);
                                    pin.Cursor = Cursors.Hand;
                                    newLayer.AddChild(pin, p.Location, PositionOrigin.BottomCenter);

                                    ToolTip tt1 = AddToolTip(label.ToString());
                                    ToolTipService.SetToolTip(pin, tt1);
                                    newLayer.Children.Remove(p);


                                }

                                if (el.GetType().Equals(typeof(MapLayer)))
                                {
                                    MapLayer p = (MapLayer)el;
                                    p.Cursor = Cursors.Hand;
                                    foreach (MapPolygon mp in p.Children)
                                    {
                                        for (int i = 0; i < mp.Locations.Count; i++)
                                        {
                                            AllPointsInLayer.Add(mp.Locations[i]);
                                        }


                                        mp.Fill = new SolidColorBrush(MiscFunctions.ColorFromInt(LayerStyle[Layers[0].Name].fill));
                                        mp.Stroke = new SolidColorBrush(MiscFunctions.ColorFromInt(LayerStyle[Layers[0].Name].stroke));
                                        mp.Opacity = LayerStyle[Layers[0].Name].opacity;

                                        LocationRect rect = new LocationRect(mp.Locations);

                                        Pin pin = new Pin { ShowCustomTree = true };
                                        pin.Tag = (el as MapLayer);
                                        ToolTip tt1 = AddToolTip(label.ToString());
                                        pin.Cursor = Cursors.Hand;
                                        ToolTipService.SetToolTip(pin, tt1);
                                        pin.MouseLeftButtonUp += new MouseButtonEventHandler(el_MouseLeftButtonUp);

                                        newLayer.AddChild(pin, rect.Center, PositionOrigin.BottomCenter);

                                        photoservices.GetFotos(rect, Layers[2], currentMap);
                                    }
                                }

                                if (el.GetType().Equals(typeof(MapPolyline)))
                                {
                                    MapPolyline p = (MapPolyline)el;
                                    for (int i = 0; i < p.Locations.Count; i++)
                                    {
                                        AllPointsInLayer.Add(p.Locations[i]);
                                    }
                                    p.Stroke = new SolidColorBrush(MiscFunctions.ColorFromInt(LayerStyle[Layers[0].Name].stroke));
                                    p.StrokeThickness = 2;
                                    p.StrokeMiterLimit = 0;
                                }
                                if (el.GetType().Equals(typeof(MapPolygon)))
                                {

                                    MapPolygon p = (MapPolygon)el;
                                    for (int i = 0; i < p.Locations.Count; i++)
                                    {
                                        AllPointsInLayer.Add(p.Locations[i]);
                                    }
                                    LocationRect rect = new LocationRect(p.Locations);
                                    Pin pin = new Pin { ShowCustomTree = true };
                                    ToolTip tt1 = AddToolTip(label.ToString());
                                    ToolTipService.SetToolTip(pin, tt1);
                                    pin.Cursor = Cursors.Hand;
                                    pin.MouseLeftButtonUp += new MouseButtonEventHandler(el_MouseLeftButtonUp);

                                    newLayer.AddChild(pin, rect.Center, PositionOrigin.BottomCenter);
                                    p.Stroke = new SolidColorBrush(MiscFunctions.ColorFromInt(LayerStyle[Layers[0].Name].stroke));
                                    p.Fill = new SolidColorBrush(MiscFunctions.ColorFromInt(LayerStyle[Layers[0].Name].fill));
                                    p.Opacity = LayerStyle[Layers[0].Name].opacity;
                                }
                            }
                        }
                    }
                    CenterRectAreas = new LocationRect(AllPointsInLayer);
                    if(currentMap != null)
                    currentMap.SetView(CenterRectAreas);
                    if (AllPointsInLayer != null)
                    AllPointsInLayer.Clear();
                    CenterRectAreas = null;
                }
                else
                {
                    cw = new ChildWindow { Content = "Too many locations were found, try a smaller area" };
                    cw.Show();
                }
            }
            else
            {
                cw = new ChildWindow { Content = string.Format("No locations were found, try a bigger area {0}",e.OutputMessage) };
                cw.Show();
            }
            Messenger.Default.Send<bool>(true, "draw");
        }

        void el_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is MapLayer)
            {
                SelectedArea = (sender as MapLayer);
                
            }
            else
            {
                if (sender is Pin)
                {
                
                    SelectedArea = ((sender as Pin).Tag as MapLayer);
                }
            }


            currentMap.Center = currentMap.ViewportPointToLocation(e.GetPosition(currentMap));

            VisualStateName = "PhotoState";
        }

        #endregion

        #region UI Stuff

        /// <summary>
        /// event handler for polygon mouse enter hint
        /// reduces opacity on enter
        /// </summary>
        private void polygon_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is UIElement)
            {
                UIElement p = sender as UIElement;
                p.Opacity = 0.5;
                //p.Projection.SetValue(PlaneProjection.GlobalOffsetZProperty, 200.0);
            }
        }

        /// <summary>
        /// event handler for polygon mouse leave
        /// returns to original opacity
        /// </summary>
        private void polygon_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is UIElement)
            {
                UIElement p = sender as UIElement;
                //p.Projection.SetValue(PlaneProjection.GlobalOffsetZProperty, 0.0);
                p.Opacity = 1;
            }
        }

        /// <summary>
        /// event handler for point mouse enter hint
        /// sets a ZProperty value (Brings pushpin out of page, closer to viewer)
        /// </summary>
        private void pushpin_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is UIElement)
            {
                UIElement pin = sender as UIElement;
                pin.Projection.SetValue(PlaneProjection.GlobalOffsetZProperty, 200.0);
            }
        }


        /// <summary>
        /// event handler for point mouse leave
        /// sets a ZProperty value back to 0.0
        /// </summary>
        private void pushpin_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is UIElement)
            {
                UIElement pin = sender as UIElement;
                pin.Projection.SetValue(PlaneProjection.GlobalOffsetZProperty, 0.0);

            }
        }

        /// <summary>
        /// Add ToolTip with styling parameters
        /// </summary>
        private ToolTip AddToolTip(string fields)
        {
            ToolTip tt = new ToolTip();
            tt.Opacity = 0.75;
            tt.Background = new SolidColorBrush(MiscFunctions.ColorFromInt("#FF222222"));
            tt.Foreground = new SolidColorBrush(Colors.White);
            tt.BorderBrush = new SolidColorBrush(Colors.Black);
            tt.Padding = new Thickness(5);
            tt.Placement = PlacementMode.Mouse;
            tt.Content = fields;
            return tt;
        }
     
        #endregion

        #region Properties


        /// <summary>
        /// The <see cref="IsBusy" /> property's name.
        /// </summary>
        public const string IsBusyPropertyName = "IsBusy";

        private bool _isbusy = false;

        /// <summary>
        /// Gets the IsBusy property.
        /// TODO Update documentation:
        /// Changes to that property's value raise the PropertyChanged event. 
        /// This property's value is broadcasted by the Messenger's default instance when it changes.
        /// </summary>
        public bool IsBusy
        {
            get
            {
                return _isbusy;
            }

            set
            {
                if (_isbusy == value)
                {
                    return;
                }

                var oldValue = _isbusy;
                _isbusy = value;


                // Update bindings, no broadcast
                RaisePropertyChanged(IsBusyPropertyName);


            }
        }


        /// <summary>
        /// The <see cref="SelectedArea" /> property's name.
        /// </summary>
        public const string SelectedAreaPropertyName = "SelectedArea";

        private MapLayer _selectedArea = null;

        /// <summary>
        /// Gets the SelectedArea property.
        /// TODO Update documentation:
        /// Changes to that property's value raise the PropertyChanged event. 
        /// This property's value is broadcasted by the Messenger's default instance when it changes.
        /// </summary>
        public MapLayer SelectedArea
        {
            get
            {
                return _selectedArea;
            }

            set
            {
                if (_selectedArea == value)
                {
                    return;
                }

                var oldValue = _selectedArea;
                _selectedArea = value;

                if(SelectedArea != null)
                if(_selectedArea.Children.Count > 0 && _selectedArea.Children[0] is MapPolygon)
                currentMap.Center = new LocationRect((_selectedArea.Children[0] as MapPolygon).Locations).Center;

                     // Update bindings and broadcast change using GalaSoft.MvvmLight.Messenging
                RaisePropertyChanged(SelectedAreaPropertyName, oldValue, value, true);
            }
        }

      
private string query = string.Empty;


     

        /// <summary>
        /// The <see cref="CurrentUser" /> property's name.
        /// </summary>
        public const string CurrentUserPropertyName = "CurrentUser";
        private SiteUser _currentUSer = null;
        /// <summary>
        /// Gets the CurrentUser property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public SiteUser CurrentUser
        {
            get
            {
                return _currentUSer;
            }

            set
            {
                if (_currentUSer == value)
                {
                    return;
                }

                var oldValue = _currentUSer;
                _currentUSer = value;


                // Update bindings, no broadcast
                RaisePropertyChanged(CurrentUserPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="CurrentCenter" /> property's name.
        /// </summary>
        public const string CurrentCenterPropertyName = "CurrentCenter";
        private Point _currentCenter = new Point(0, 0);
        /// <summary>
        /// Gets the CurrentCenter property.
        /// TODO Update documentation:
        /// Changes to that property's value raise the PropertyChanged event. 
        /// This property's value is broadcasted by the Messenger's default instance when it changes.
        /// </summary>
        public Point CurrentCenter
        {
            get
            {
                return _currentCenter;
            }

            set
            {
                if (_currentCenter == value)
                {
                    return;
                }

                var oldValue = _currentCenter;
                _currentCenter = value;


                // Update bindings, no broadcast
                RaisePropertyChanged(CurrentCenterPropertyName);

            }
        }

        /// <summary>
        /// The <see cref="CurrentZoomLevel" /> property's name.
        /// </summary>
        public const string CurrentZoomLevelPropertyName = "CurrentZoomLevel";

        private double _currentZoomLevel = 4;

        /// <summary>
        /// Gets the CurrentZoomLevel property.
        /// TODO Update documentation:
        /// Changes to that property's value raise the PropertyChanged event. 
        /// This property's value is broadcasted by the Messenger's default instance when it changes.
        /// </summary>
        public double CurrentZoomLevel
        {
            get
            {
                return _currentZoomLevel;
            }

            set
            {
                if (_currentZoomLevel == value)
                {
                    return;
                }

                var oldValue = _currentZoomLevel;
                _currentZoomLevel = value;


                // Update bindings, no broadcast
                RaisePropertyChanged(CurrentZoomLevelPropertyName);

                // Update bindings and broadcast change using GalaSoft.MvvmLight.Messenging
                //  RaisePropertyChanged(CurrentZoomLevelPropertyName, oldValue, value, true);
            }
        }


        /// <summary>
        /// The <see cref="Layers" /> property's name.
        /// </summary>
        public const string LayersPropertyName = "Layers";

        private ObservableCollection<MapLayer> _layers = null;

        /// <summary>
        /// Gets the Layers property.
        /// TODO Update documentation:
        /// Changes to that property's value raise the PropertyChanged event. 
        /// This property's value is broadcasted by the Messenger's default instance when it changes.
        /// </summary>
        public ObservableCollection<MapLayer> Layers
        {
            get
            {
                return _layers;
            }

            set
            {
                if (_layers == value)
                {
                    return;
                }

                var oldValue = _layers;
                _layers = value;


                // Update bindings, no broadcast
                RaisePropertyChanged(LayersPropertyName);

                // Update bindings and broadcast change using GalaSoft.MvvmLight.Messenging
                // RaisePropertyChanged(LayersPropertyName, oldValue, value, true);
            }
        }

        public const string VisualStateNamePropertyName = "VisualStateName";

        private string _visualStateName = "DefaultState";

        /// <summary>
        /// Gets the VisualStateName property.
        /// TODO Update documentation:
        /// Changes to that property's value raise the PropertyChanged event. 
        /// This property's value is broadcasted by the Messenger's default instance when it changes.
        /// </summary>
        public string VisualStateName
        {
            get
            {
                return _visualStateName;
            }

            set
            {
                if (_visualStateName == value)
                {
                    return;
                }

                var oldValue = _visualStateName;
                _visualStateName = value;


                // Update bindings, no broadcast
                RaisePropertyChanged(VisualStateNamePropertyName);

            }
        }


        public override void Cleanup()
        {
            cw = null;
            this.currentMap = null;
            SelectedArea = null;
            this.Layers.Clear();
            Messenger.Default.Unregister(this);
            base.Cleanup();
        }

        #endregion

    }
}