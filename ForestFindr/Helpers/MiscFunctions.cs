using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Microsoft.Maps.MapControl;

namespace ForestFindr.Helpers
{
    public static class MiscFunctions
    {

        /// <summary>
        /// Map resolution formula from Bing Maps site:
        ///     http://msdn.microsoft.com/en-us/library/aa940990.aspx
        /// </summary>
        /// <param name="latitude">decimal latitude</param>
        /// <param name="zoomlevel">double zoom level</param>
        /// <returns></returns>
        public static double GetMapResolution(double latitude, double zoomlevel)
        {
            return 156543.04 * Math.Cos(latitude * 0.0174532925) / Math.Pow(2.0, zoomlevel);
        }


        /// <summary>
        /// HaversineDistance
        ///     Calculates shortest surface distance between to points
        ///     for a Spheroidal earth model
        ///     ref: http://www.movable-type.co.uk/scripts/latlong.html
        /// </summary>
        /// <param name="P1">Location lat,lon</param>
        /// <param name="P2">Location lat,lon</</param>
        /// <returns>double distance</returns>
        public static double HaversineDistance(Location P1, Location P2)
        {
            double EarthRadius = 6378137.0;
            double ToRadians = Math.PI / 180;

            double dLat = (P2.Latitude - P1.Latitude) * ToRadians;
            double dLon = (P2.Longitude - P1.Longitude) * ToRadians;
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                     Math.Cos(P1.Latitude * ToRadians) * Math.Cos(P2.Latitude * ToRadians) *
                     Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return EarthRadius * c;
        }


        /// <summary>
        /// ColorFromInt
        /// Returns a Color from hex string i.e. #FF00FF00
        /// </summary>
        /// <param name="hex">string hex color with alpha, red, green, blue</param>
        /// <returns>Color</returns>
        public static Color ColorFromInt(string hex)
        {
            if (hex.StartsWith("#")) hex = hex.Substring(1);
            int c = int.Parse(hex, NumberStyles.AllowHexSpecifier);
            return Color.FromArgb((byte)((c >> 0x18) & 0xff),
                (byte)((c >> 0x10) & 0xff),
                (byte)((c >> 8) & 0xff),
                (byte)(c & 0xff));
        }

      
        static double EarthRadius = 6378137.0;
        static double ToRadians = Math.PI / 180;
        static double ToDegrees = 180 / Math.PI;
        static double minLat = -85.0;
        static double maxLat = 85.0;
        static double minLon = -180.0;
        static double maxLon = 180.0;

        /// <summary>
        /// Clips a number to the specified minimum and maximum values.
        /// </summary>
        /// <param name="n">The number to clip.</param>
        /// <param name="minValue">Minimum allowable value.</param>
        /// <param name="maxValue">Maximum allowable value.</param>
        /// <returns>The clipped value.</returns>
        private static double Clip(double n, double minValue, double maxValue)
        {
            return Math.Min(Math.Max(n, minValue), maxValue);
        }


        /// <summary>
        ///  Spherical Mercator Projection conversion  
        ///  using EPSG:3457 parameters 
        ///  matches Bing Maps coordinate system
        /// </summary>
        /// <param name="lon">double longitude</param>
        /// <param name="lat">double latitude</param>
        /// <returns>Point in Mercator X,Y</returns>
        public static Point Mercator(double lon, double lat)
        {
            /* spherical mercator 
             * epsg:900913 R= 6378137 
             * x = longitude
             * y= R*ln(tan(pi/4 +latitude/2)
             */
            lat = Clip(lat, minLat, maxLat);
            lon = Clip(lon, minLon, maxLon);
            double x = EarthRadius * ToRadians * lon;
            double y = EarthRadius * Math.Log(Math.Tan(ToRadians * (45 + lat / 2.0)));

            return new Point(x, y);
        }

        /// <summary>
        /// Inverse Spherical Mercator Projection conversion  
        ///   using EPSG:3457 parameters 
        ///   matches Bing Maps coordinate system
        /// </summary>
        /// <param name="x">double Mercator X</param>
        /// <param name="y">double Mercator Y</param>
        /// <returns>Point longitude,latitude</returns>
        /// <remarks>
        /// Note order of (longitude,latitude) it is not the same as Location order (lat,lon)
        /// </remarks>
        public static Point InverseMercator(double x, double y)
        {
            /* spherical mercator for Google, VE, Yahoo etc
             * epsg:900913 R= 6378137 
             */
            double lon = x / EarthRadius * ToDegrees;
            double lat = Math.Atan(Math.Sinh(y / EarthRadius)) * ToDegrees;
            return new Point(lon, lat);
        }

    }
}
