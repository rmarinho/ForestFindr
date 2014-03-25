using System.Collections.Generic;
using System.Windows.Browser;
using System;
using System.Text;

namespace PanoramioNet
{

    public class Panoramio
    {

        private static string baseurl = "http://www.panoramio.com/map/get_panoramas.php?set=public&callback=?";

        public Panoramio()
        {
            HtmlPage.RegisterScriptableObject("PanoramioNet", this);
        }
        public void GetPhotos(BoundaryBox bbox, string size, int from, int to)
        {
            StringBuilder str = new StringBuilder(baseurl);

            str.Append(string.Format("&minx={0}&miny={1}&maxx={2}&maxy={3}", bbox.MinimumLongitude, bbox.MinimumLatitude, bbox.MaximumLongitude, bbox.MaximumLatitude));

            str.Append(string.Format("&from={0}&to={1}", from, to));
           

            //call jquery on the default aspx , panoramio doesn't have a clientaccesspolicy, the use jason with padding
            if (size.Equals("square"))
            {
                str.Append("&mapfilter=true&size=square");
                HtmlPage.Window.Invoke("GetPanoramioPhotos", str.ToString());

            }
          
         
        }

        public void GetPhotosmedium(BoundaryBox bbox, string size, int from, int to)
        {
            StringBuilder str = new StringBuilder(baseurl);

            str.Append(string.Format("&minx={0}&miny={1}&maxx={2}&maxy={3}", bbox.MinimumLongitude, bbox.MinimumLatitude, bbox.MaximumLongitude, bbox.MaximumLatitude));

            str.Append(string.Format("&from={0}&to={1}", from, to));
            if (size.Equals("medium"))
            {
                str.Append("&size=" + size);
                HtmlPage.Window.Invoke("GetMediumPanoramioPhotos", str.ToString());
            }
        }
        [ScriptableMember()]
        public void ProcessPhotosCallback(ScriptObject json)
        {
            OnPhotoCollectionCompleted(new PhotoCollectionEventArgs(json.ConvertTo<PhotoCollection>()));

        }

        [ScriptableMember()]
        public void ProcessMediumPhotosCallback(ScriptObject json)
        {
            OnMediumPhotoCollectionCompleted(new PhotoCollectionEventArgs(json.ConvertTo<PhotoCollection>()));

        }
        public event PhotoCollectionHandler GetMediumPhotoCollectionCompleted;
        public event PhotoCollectionHandler GetPhotoCollectionCompleted;

        public delegate void PhotoCollectionHandler(object sender, PhotoCollectionEventArgs e);
        public class PhotoCollectionEventArgs : EventArgs
        {
            public PhotoCollectionEventArgs(PhotoCollection photos)
            {
                this.PhotosCollection = photos;

            }

            private PhotoCollection _photos;
            public PhotoCollection PhotosCollection
            {
                get { return _photos; }
                set { _photos = value; }
            }
        }
    
        protected virtual void OnPhotoCollectionCompleted(PhotoCollectionEventArgs e)
        {
            if (GetPhotoCollectionCompleted != null) { GetPhotoCollectionCompleted(this, e); }
        }

        protected virtual void OnMediumPhotoCollectionCompleted(PhotoCollectionEventArgs e)
        {
            if (GetMediumPhotoCollectionCompleted != null) { GetMediumPhotoCollectionCompleted(this, e); }
        }
    }
}