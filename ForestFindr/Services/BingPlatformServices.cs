using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.Windows;
using System.Windows.Browser;
using System.Windows.Controls;
using System.Windows.Media;
using ForestFindr.GeocodeService;
using ForestFindr.Helpers;
using ForestFindr.RouteService;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Maps.MapControl;

namespace ForestFindr.Services
{
    public  class BingPlatformServices
    {

        private object lockObject = new object();

        public void GetRoute(string FromOutput, string ToOutput)
        {

            // Geocode locations in parallel.
            GeocodeResult[] results = new GeocodeResult[2];
            // From location.
            RoutingState state0 = new RoutingState(results, 0);
            GeocodeAddress(FromOutput, state0);
            // To location.
            RoutingState state1 = new RoutingState(results, 1);
            GeocodeAddress(ToOutput, state1);

        }

        private void CalculateRoute(GeocodeResult[] locations)
        {
            RouteRequest request = new RouteRequest();
            request.Waypoints = new ObservableCollection<Waypoint>();
            foreach (GeocodeResult result in locations)
            {
                request.Waypoints.Add(GeocodeResultToWaypoint(result));
            }

            // Don't raise exceptions.
            request.ExecutionOptions = new RouteService.ExecutionOptions();
            request.ExecutionOptions.SuppressFaults = true;

            // Only accept results with high confidence.
            request.Options = new RouteOptions();
            request.Options.RoutePathType = RoutePathType.Points;

            ((ClientTokenCredentialsProvider)App.Current.Resources["MyCredentials"]).GetCredentials(
                (Credentials credentials) =>
                {
                    //Pass in credentials for web services call.
                    //Replace with your own Credentials.
                    request.Credentials = credentials;

                    // Make asynchronous call to fetch the data ... pass state object.
                    RouteClient.CalculateRouteAsync(request);
                });
        }

        private GeocodeServiceClient geocodeClient;
        private GeocodeServiceClient GeocodeClient
        {
            get
            {
                if (null == geocodeClient)
                {
                    //Handle http/https; OutOfBrowser is currently supported on the MapControl only for http pages
                    bool httpsUriScheme = !Application.Current.IsRunningOutOfBrowser && HtmlPage.Document.DocumentUri.Scheme.Equals(Uri.UriSchemeHttps);
                    BasicHttpBinding binding = new BasicHttpBinding(httpsUriScheme ? BasicHttpSecurityMode.Transport : BasicHttpSecurityMode.None);
                    UriBuilder serviceUri = new UriBuilder("http://dev.virtualearth.net/webservices/v1/GeocodeService/GeocodeService.svc");
                    if (httpsUriScheme)
                    {
                        //For https, change the UriSceheme to https and change it to use the default https port.
                        serviceUri.Scheme = Uri.UriSchemeHttps;
                        serviceUri.Port = -1;
                    }

                    //Create the Service Client
                    geocodeClient = new GeocodeServiceClient(binding, new EndpointAddress(serviceUri.Uri));
                    geocodeClient.GeocodeCompleted += new EventHandler<GeocodeCompletedEventArgs>(geocodeClient_GeocodeCompleted);
                }
                return geocodeClient;
            }
        }

        void geocodeClient_GeocodeCompleted(object sender, GeocodeCompletedEventArgs e)
        {
            RoutingState state = e.UserState as RoutingState;
            GeocodeResult result = null;
            string outString;

            try
            {
                if (e.Result.ResponseSummary.StatusCode != GeocodeService.ResponseStatusCode.Success)
                {
                    outString = "error geocoding ... status <" + e.Result.ResponseSummary.StatusCode.ToString() + ">";
                }
                else if (0 == e.Result.Results.Count)
                {
                    outString = "No result";
                }
                else
                {
                    // Only report on first result.
                    result = e.Result.Results[0];
                    outString = result.DisplayName;
                }
            }
            catch (Exception)
            {
                outString = "Exception raised";
            }

            // Update UI with geocode result.
            if (null != state.output)
            {
                state.output.Text = outString;
            }

            if (null == result)
            {
                result = new GeocodeResult();
            }

            // Update state object ... when all the results are set, call route.
            bool doneGeocoding;
            lock (lockObject)
            {
                state.results[state.locationNumber] = result;
                doneGeocoding = state.GeocodesComplete;
            }

            if (doneGeocoding && state.GeocodesSuccessful)
            {
                ////Clear any existing routes
                //ClearRoute();

                ////Calculate the route
                CalculateRoute(state.results);
            }

        }
        private RouteServiceClient routeClient;
        private RouteServiceClient RouteClient
        {
            get
            {
                if (null == routeClient)
                {
                    //Handle http/https; OutOfBrowser is currently supported on the MapControl only for http pages
                    bool httpsUriScheme = !Application.Current.IsRunningOutOfBrowser && HtmlPage.Document.DocumentUri.Scheme.Equals(Uri.UriSchemeHttps);
                    BasicHttpBinding binding = new BasicHttpBinding(httpsUriScheme ? BasicHttpSecurityMode.Transport : BasicHttpSecurityMode.None);
                    binding.MaxReceivedMessageSize = int.MaxValue;
                    binding.MaxBufferSize = int.MaxValue;
                    UriBuilder serviceUri = new UriBuilder("http://dev.virtualearth.net/webservices/v1/RouteService/RouteService.svc");
                    if (httpsUriScheme)
                    {
                        //For https, change the UriSceheme to https and change it to use the default https port.
                        serviceUri.Scheme = Uri.UriSchemeHttps;
                        serviceUri.Port = -1;
                    }

                    //Create the Service Client
                    routeClient = new RouteServiceClient(binding, new EndpointAddress(serviceUri.Uri));
                    routeClient.CalculateRouteCompleted += new EventHandler<CalculateRouteCompletedEventArgs>(routeClient_CalculateRouteCompleted);
                }
                return routeClient;
            }
        }

        void routeClient_CalculateRouteCompleted(object sender, CalculateRouteCompletedEventArgs e)
        {
            string outString;
            try
            {
                if (e.Result.ResponseSummary.StatusCode != RouteService.ResponseStatusCode.Success)
                {
                    outString = "error routing ... status <" + e.Result.ResponseSummary.StatusCode.ToString() + ">";
                }
                else if (0 == e.Result.Result.Legs.Count)
                {
                    outString = "Cannot find route";
                }
                else
                {
                    LocationCollection coll = new LocationCollection();
                    foreach (Location p in e.Result.Result.RoutePath.Points)
                    {
                        coll.Add(new Location(p.Latitude, p.Longitude));
                    }

                    Messenger.Default.Send<Messages.RouteDoneMessage>(new Messages.RouteDoneMessage { Legs = e.Result.Result.Legs, Locations = coll}, "RouteDone");
                  
                }
            }
            catch (Exception)
            {
                outString = "Exception raised routine";
            }

            // ToOutput.Text = outString;

        }


        private Waypoint GeocodeResultToWaypoint(GeocodeResult result)
        {
            Waypoint waypoint = new Waypoint();
            waypoint.Description = result.DisplayName;
            waypoint.Location = new Location();
            waypoint.Location.Latitude = result.Locations[0].Latitude;
            waypoint.Location.Longitude = result.Locations[0].Longitude;
            return waypoint;
        }

        private void GeocodeAddress(string address, RoutingState state)
        {
            GeocodeRequest request = new GeocodeRequest();
            request.Query = address;
            // Don't raise exceptions.
            request.ExecutionOptions = new GeocodeService.ExecutionOptions();
            request.ExecutionOptions.SuppressFaults = true;

            // Only accept results with high confidence.
            request.Options = new GeocodeService.GeocodeOptions();
            // Using ObservableCollection since this is the default for Silverlight proxy generation.
            request.Options.Filters = new ObservableCollection<FilterBase>();
            ConfidenceFilter filter = new ConfidenceFilter();
            filter.MinimumConfidence = GeocodeService.Confidence.High;
            request.Options.Filters.Add(filter);

            if (null != state.output)
            {
                state.output.Text = "<geocoding in progress>";
                state.output.Foreground = new SolidColorBrush(Colors.Black);
            }

            //it's in the resources already
            ((ClientTokenCredentialsProvider)App.Current.Resources["MyCredentials"]).GetCredentials(
                (Credentials credentials) =>
                {
                    // Pass in credentials for web services call.
                    //Replace with your own Credentials.
                    request.Credentials = credentials;

                    // Make asynchronous call to fetch the data ... pass state object.
                    GeocodeClient.GeocodeAsync(request, state);
                });


         

         
        }

    }

    internal class RoutingState
    {
        internal RoutingState(GeocodeResult[] resultArray, int index)
        {
            results = resultArray;
            locationNumber = index;
          
        }

        internal bool GeocodesComplete
        {
            get
            {
                for (int idx = 0; idx < results.Length; idx++)
                {
                    if (null == results[idx])
                        return false;
                }
                return true;
            }
        }

        internal bool GeocodesSuccessful
        {
            get
            {
                for (int idx = 0; idx < results.Length; idx++)
                {
                    if (null == results[idx] || null == results[idx].Locations || 0 == results[idx].Locations.Count)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        internal GeocodeResult[] results;
        internal int locationNumber;
        internal TextBlock output;
    }

}
