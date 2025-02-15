namespace BreweryFinder.API.Models
{
    public class Brewery
    {
        public required string Name { get; set; }
        public required string BreweryType { get; set; }
        public required string Street { get; set; }
        public required string City { get; set; }
        public required string State { get; set; }
        public required string PostalCode { get; set; }
        public string? Phone { get; set; }
        public string? Website { get; set; }

    }
}
