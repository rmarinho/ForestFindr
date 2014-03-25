
namespace ForestFindr.Helpers
{
    public class LoadStyle
    {

        private string _stroke = string.Empty;
        private string _fill = string.Empty;
        private double _opacity = 0.0;

        public LoadStyle(string stroke, string fill, double opacity)
        {
            _stroke = stroke;
            _fill = fill;
            _opacity = opacity;
        }

        public string stroke
        {
            get { return _stroke; }
            set { _stroke = value; }
        }

        public string fill
        {
            get { return _fill; }
            set { _fill = value; }
        }

        public double opacity
        {
            get { return _opacity; }
            set { _opacity = value; }
        }
    }
}
