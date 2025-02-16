using Azure;
using BreweryFinder.API.Models;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

namespace BreweryFinder.API.Services;

public class BreweryService(HttpClient httpClient) : IBreweryService
{


    public async Task<List<Brewery>> GetBreweriesByState(string state)
    {
        var url = $"/v1/breweries?by_state={state}";
        using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();
        var stream = await response.Content.ReadAsStreamAsync();
        var breweries = await JsonSerializer.DeserializeAsync<List<Brewery>>(stream);
        return breweries ?? [];
    }

    public void Dispose() => httpClient?.Dispose();
}
