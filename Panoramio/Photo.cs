using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace PanoramioNet
{
    public class Photo
    {
        public int photo_id { get; set; }

        public string photo_title { get; set; }
        public string photo_url { get; set; }
        public string photo_file_url { get; set; }
        public double longitude { get; set; }
        public double latitude { get; set; }
        public double width { get; set; }
        public double height { get; set; }
        public string upload_date { get; set; }
        public int owner_id { get; set; }
        public string owner_name { get; set; }
        public string owner_url { get; set; }
    }
}