
namespace ForestFindr.Web.Model
{
    using System.ComponentModel.DataAnnotations;


    // The MetadataTypeAttribute identifies CountryMetadata as the class
    // that carries additional metadata for the Country class.
    [MetadataTypeAttribute(typeof(Country.CountryMetadata))]
    public partial class Country
    {

        // This class allows you to attach custom attributes to properties
        // of the Country class.
        //
        // For example, the following marks the Xyz property as a
        // required property and specifies the format for valid values:
        //    [Required]
        //    [RegularExpression("[A-Z][A-Za-z0-9]*")]
        //    [StringLength(32)]
        //    public string Xyz { get; set; }
        internal sealed class CountryMetadata
        {

            // Metadata classes are not meant to be instantiated.
            private CountryMetadata()
            {
            }

            public int id { get; set; }

            public string name { get; set; }

            public string name_iso { get; set; }
        }
    }
}
