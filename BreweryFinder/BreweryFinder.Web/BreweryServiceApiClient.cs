using BreweryFinder.API.Models;
using System.Text.Json;

namespace BreweryFinder.Web;

public class BreweryServiceApiClient(HttpClient httpClient)
{
    public async Task<List<Brewery>> GetBreweriesAsync(BrewerySearchCriteria searchCriteria)
    {
        using var response = await httpClient.PostAsJsonAsync("api/BreweryFinder/search", searchCriteria);

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<Brewery>>(content) ?? new List<Brewery>();
    }

    public async Task<List<Brewery>> GetBreweriesByCityAsync(string city)
    {
        var response = await httpClient.GetAsync($"api/breweries/byCity?city={city}");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<Brewery>>(content) ?? new List<Brewery>();
    }

    public async Task<List<Brewery>> GetBreweriesByStateAsync(string state)
    {
        var response = await httpClient.GetAsync($"api/breweries/byState?state={state}");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<Brewery>>(content) ?? new List<Brewery>();
    }

    public async Task<List<Brewery>> GetBreweriesByTypeAsync(string type)
    {
        var response = await httpClient.GetAsync($"api/breweries/byType?type={type}");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<Brewery>>(content) ?? new List<Brewery>();
    }
}