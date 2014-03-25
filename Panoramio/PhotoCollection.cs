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
using System.Collections.Generic;

namespace PanoramioNet
{
    public class PhotoCollection
    {

        private int _count;
        public int count
        {
            get { return _count; }
            set { _count = value; }
        }


        private bool _has_more;
        public bool has_more
        {
            get { return _has_more; }
            set { _has_more = value; }
        }
    
        private List<Photo> _photos;
        public List<Photo> photos
        {
            get { return _photos; }
            set { _photos = value; }
        }
    }
}
