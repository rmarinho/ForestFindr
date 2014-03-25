using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Maps.MapControl;
using ForestFindr.Entities;
using System.Collections.Generic;
using ForestFindr.WIKIpediaService;

namespace ForestFindr.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm/getstarted
    /// </para>
    /// </summary>
    [Export]
    public class PhotosViewModel : ViewModelBase
    {
        #region Wikipedia Client Proxy

        private WIKIClient wikiClient = new WIKIClient(); 
        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the PhotosViewModel class.
        /// </summary>
        public PhotosViewModel()
        {

            wikiClient.GetWikiOpenSearchCompleted += new System.EventHandler<GetWikiOpenSearchCompletedEventArgs>(wikiClient_GetWikiOpenSearchCompleted);
            Messenger.Default.Register<PropertyChangedMessage<MapLayer>>(this,
                      (action) =>
                      {
                          if (action.NewValue != null)
                          {
                              CurrentArea = (action.NewValue.Tag as Area);
                              GetPhotos(action.NewValue);
                              CurrentWikipediaResult = null;
                              wikiClient.GetWikiOpenSearchAsync(CurrentArea.Name, 10, CurrentArea.Name);
                          }

                          else
                          {
                              Photos.Clear();
                          }
                      });


        } 
        #endregion
        
        #region GetWikipedia Info

        void wikiClient_GetWikiOpenSearchCompleted(object sender, GetWikiOpenSearchCompletedEventArgs e)
        {
            if (e.Error == null && e.Result != null)
            {
                if (e.Result.Count > 0)
                    CurrentWikipediaResult = e.Result[0];
                else
                    CurrentWikipediaResult = new WikipediaOpenSearchResult { Description = "no wikipedia article found" };
            }

        }

        
        #endregion

        #region GetPanoramio Photos
        private void GetPhotos(MapLayer layer)
        {

            IsBusy = true;


            //this is not right 
            PanoramioNet.Panoramio panoramioApi = new PanoramioNet.Panoramio();
            panoramioApi.GetMediumPhotoCollectionCompleted += new PanoramioNet.Panoramio.PhotoCollectionHandler(panoramioApi_GetMediumPhotoCollectionCompleted);

            if (Photos == null)
                Photos = new ObservableCollection<PhotoItem>();
            else
                Photos.Clear();
            foreach (var mp in layer.Children)
            {
                if (mp is MapPolygon)
                {

                    LocationRect rec = new LocationRect((mp as MapPolygon).Locations);
                    PanoramioNet.BoundaryBox PanoramioOptions = new PanoramioNet.BoundaryBox
                    {
                        MaximumLatitude = rec.North,
                        MinimumLatitude = rec.South,
                        MaximumLongitude = rec.East,
                        MinimumLongitude = rec.West
                    };
                    panoramioApi.GetPhotosmedium(PanoramioOptions, "medium", 0, 10);
                }
            }
        }

        void panoramioApi_GetMediumPhotoCollectionCompleted(object sender, PanoramioNet.Panoramio.PhotoCollectionEventArgs e)
        {
            foreach (var item in e.PhotosCollection.photos)
            {
                Photos.Add(new PhotoItem { Name = item.photo_title, PhotoUrl = new System.Uri(item.photo_file_url) });
            }

            IsBusy = false;

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
        /// The <see cref="CurrentWikipediaResult" /> property's name.
        /// </summary>
        public const string CurrentWikipediaResultPropertyName = "CurrentWikipediaResult";

        private WikipediaOpenSearchResult _currentWikipediaResult = null;

        /// <summary>
        /// Gets the CurrentWikipediaResult property.
        /// TODO Update documentation:
        /// Changes to that property's value raise the PropertyChanged event. 
        /// This property's value is broadcasted by the Messenger's default instance when it changes.
        /// </summary>
        public WikipediaOpenSearchResult CurrentWikipediaResult
        {
            get
            {
                return _currentWikipediaResult;
            }

            set
            {
                if (_currentWikipediaResult == value)
                {
                    return;
                }

                var oldValue = _currentWikipediaResult;
                _currentWikipediaResult = value;


                // Update bindings, no broadcast
                RaisePropertyChanged(CurrentWikipediaResultPropertyName);


            }
        }

        /// <summary>
        /// The <see cref="Photos" /> property's name.
        /// </summary>
        public const string PhotosPropertyName = "Photos";

        private ObservableCollection<PhotoItem> _photos = null;

        /// <summary>
        /// Gets the Photos property.
        /// TODO Update documentation:
        /// Changes to that property's value raise the PropertyChanged event. 
        /// This property's value is broadcasted by the Messenger's default instance when it changes.
        /// </summary>
        public ObservableCollection<PhotoItem> Photos
        {
            get
            {
                return _photos;
            }

            set
            {
                if (_photos == value)
                {
                    return;
                }

                var oldValue = _photos;
                _photos = value;

                // Remove one of the two calls below
                //  throw new NotImplementedException();

                // Update bindings, no broadcast
                RaisePropertyChanged(PhotosPropertyName);

                // Update bindings and broadcast change using GalaSoft.MvvmLight.Messenging
                // RaisePropertyChanged(PhotosPropertyName, oldValue, value, true);
            }
        }

        /// <summary>
        /// The <see cref="VisualStateName" /> property's name.
        /// </summary>
        public const string VisualStateNamePropertyName = "VisualStateName";

        private string _visualStateName = "Default";

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

        /// <summary>
        /// The <see cref="CurrentArea" /> property's name.
        /// </summary>
        public const string CurrentAreaPropertyName = "CurrentArea";

        private Area _currentArea = null;

        /// <summary>
        /// Gets the CurrentArea property.
        /// TODO Update documentation:
        /// Changes to that property's value raise the PropertyChanged event. 
        /// This property's value is broadcasted by the Messenger's default instance when it changes.
        /// </summary>
        public Area CurrentArea
        {
            get
            {
                return _currentArea;
            }

            set
            {
                if (_currentArea == value)
                {
                    return;
                }

                var oldValue = _currentArea;
                _currentArea = value;

                // Update bindings, no broadcast
                RaisePropertyChanged(CurrentAreaPropertyName);

            }
        }

        
        #endregion

        public override void Cleanup()
        {
            //laurent rocks!
            if(Photos != null) 
            Photos.Clear();
            
            // Clean own resources if needed

            base.Cleanup();
        }
    }
}