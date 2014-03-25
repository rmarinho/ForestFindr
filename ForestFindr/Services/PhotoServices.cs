using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FlickrNet;
using ForestFindr.Controls;
using ForestFindr.Helpers;
using Microsoft.Maps.MapControl;
using PanoramioNet;

namespace ForestFindr.Services
{
    public class PhotoServices
    {

   
        #region Public Method

        public void GetFotos(LocationRect rect, MapLayer phtosLayer, CustomMap mp)
        {

            photosLayer = null;
            currentMap = null;
            currentMap = mp;
            photosLayer = phtosLayer;
            GetPhotosFlickr(rect);
            GetFotosPanoramico(rect);
        }
        
        #endregion

        #region Panoramio

        private void GetFotosPanoramico(LocationRect bbox)
        {
            PanoramioNet.BoundaryBox PanoramioOptions = new PanoramioNet.BoundaryBox
            {
                MaximumLatitude = bbox.North,
                MinimumLatitude = bbox.South,
                MaximumLongitude = bbox.East,
                MinimumLongitude = bbox.West
            };
            Panoramio PanoramioAPI = new Panoramio();
            PanoramioAPI.GetPhotoCollectionCompleted += new Panoramio.PhotoCollectionHandler(_panoramioAPI_GetPhotoCollectionCompleted);
            PanoramioAPI.GetPhotos(PanoramioOptions, "square", 0, 10);
        }

        private void _panoramioAPI_GetPhotoCollectionCompleted(object sender, Panoramio.PhotoCollectionEventArgs e)
        {
            foreach (var item in e.PhotosCollection.photos)
            {
                AddImagePushPin("http://www.dumez.nl/Images/panoramio.png", item.latitude, item.longitude, item.photo_file_url, photosLayer);

            }
        } 
        #endregion

        #region Flickr

        private void GetPhotosFlickr(LocationRect bbox)
        {

            PhotoSearchOptions options = new PhotoSearchOptions();
            options.BoundaryBox = new FlickrNet.BoundaryBox
            {
                MaximumLatitude = bbox.North,
                MinimumLatitude = bbox.South,
                MaximumLongitude = bbox.East,
                MinimumLongitude = bbox.West
            };



            options.Extras |= PhotoSearchExtras.Geo;
            FlickrApi.PhotosSearchAsync(options, new Action<FlickrResult<FlickrNet.PhotoCollection>>(photos =>
            {
                if (photos.Error == null)
                {

                    if (photos.Result.Count > 0)
                    {
                        foreach (var item in photos.Result)
                        {
                            Deployment.Current.Dispatcher.BeginInvoke(() =>
                            {
                                AddImagePushPin("http://aeisgb.comuv.com/imagens/flickr_logo.png", item.Latitude, item.Longitude, item.SquareThumbnailUrl, photosLayer);
                            }
                             );
                        }

                    }
                }

            }
              ));


        }

        private Flickr _flickrAPI;
        public Flickr FlickrApi
        {
            get
            {
                if (null == _flickrAPI)
                {
                    _flickrAPI = new Flickr("bd415295df1cde0c0d58696a05c35f69", "21637a9be44085aa");
                }

                return _flickrAPI;
            }
        }
        
        #endregion

        #region AddImageTolayer

        private static ToolTip AddToolTip(Image img)
        {
            ToolTip tt = new ToolTip();
            tt.Opacity = 0.75;
            tt.Background = new SolidColorBrush(MiscFunctions.ColorFromInt("#FF222222"));
            tt.Foreground = new SolidColorBrush(Colors.White);
            tt.BorderBrush = new SolidColorBrush(Colors.Black);
            tt.Padding = new Thickness(5);
            tt.Placement = PlacementMode.Mouse;
            tt.Content = img;
            return tt;
        }
        private static void AddImagePushPin(string urlicon, double latitude, double longitude, string url, MapLayer layer)
        {
            Pin photoPin = new Pin
            {
                ImageSource = new BitmapImage(new Uri(urlicon, UriKind.RelativeOrAbsolute)),
                MapInstance = (Map)layer.ParentMap
            };

            ToolTip tt = AddToolTip(new Image { Source = new BitmapImage(new Uri(url)) });
            ToolTipService.SetToolTip(photoPin, tt);
            layer.AddChild(photoPin, new Location(latitude, longitude, 0), PositionOrigin.Center);

        }
        
        #endregion

        #region Fields
        private MapLayer photosLayer;
        private CustomMap currentMap = null;
        #endregion
    }
}
