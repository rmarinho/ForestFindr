using System.Collections.ObjectModel;
using ForestFindr.RouteService;
using ForestFindr.Web.Model;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Maps.MapControl;

namespace ForestFindr.Helpers
{
    public static class Messages
    {

        public class RouteDoneMessage : MessageBase
        {
            public LocationCollection Locations { get; set; }
            public ObservableCollection<RouteLeg> Legs { get; set; }
        }

        public class ProximityQueryMessage : MessageBase
        {
            public LocationCollection Locations { get; set; }
            public double Radius { get; set; }
        }

        public class PolyBufferQueryMessage : MessageBase
        {
            public LocationCollection Locations { get; set; }
            public double Buffer { get; set; }
        }
        public class CountryQueryMessage : MessageBase
        {
            public Country SelectedCountry { get; set; }
        }

    }
}
