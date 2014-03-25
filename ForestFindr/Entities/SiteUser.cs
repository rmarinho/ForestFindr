
namespace ForestFindr.Entities
{
    //user that visit's the site
    public class SiteUser
    {
        public string IP { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string CountryCode { get; set; }
        public string RegionCode { get; set; }
        public string RegionName { get; set; }
        public string PostalCode { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
