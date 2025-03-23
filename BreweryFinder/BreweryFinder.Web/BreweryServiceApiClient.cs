using BreweryFinder.API.Models;

namespace BreweryFinder.Web;

public class BreweryServiceApiClient(HttpClient httpClient)
{
    public async Task<Brewery[]> GetBreweriesByCityAsync(string city)
    {
        var breweries = await httpClient.GetFromJsonAsync<Brewery[]>($"/api/breweryfinder/byCity?city={city}");
        return breweries ?? [];
    }
    public async Task<Brewery[]> GetBreweriesByStateAsync(string state)
    {
        var breweries = await httpClient.GetFromJsonAsync<Brewery[]>($"/api/breweryfinder/byState?state={state}");
        return breweries ?? [];
    }

    //this will be used to get breweries by any search criteria

    public async Task<Brewery[]> GetBreweriesAsync(BrewerySearchCriteria searchCriteria)
    {
        var breweries = await httpClient.GetFromJsonAsync<Brewery[]>($"/api/breweryfinder?{searchCriteria}");
        return breweries ?? [];
    }
}
