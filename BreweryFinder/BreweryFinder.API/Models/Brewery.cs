using System.Text.Json.Serialization;

namespace BreweryFinder.API.Models
{
    public class Brewery
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("brewery_type")]
        public string BreweryType { get; set; }

        [JsonPropertyName("street")]
        public string Street { get; set; }

        [JsonPropertyName("city")]
        public string City { get; set; }

        [JsonPropertyName("state")]
        public string State { get; set; }

        [JsonPropertyName("postal_code")]
        public string PostalCode { get; set; }

        [JsonPropertyName("phone")]
        public string? Phone { get; set; }

        [JsonPropertyName("website_url")]
        public string? Website { get; set; }
    }

    public class BrewerySearchCriteria
    {
        public string? Name { get; set; }
        public string? BreweryType { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
    }
}
