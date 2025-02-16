using BreweryFinder.API.Models;
using System.Text.Json;

namespace BreweryFinder.API.Services;

public class BreweryService(HttpClient httpClient, ILogger<BreweryService> logger) : IBreweryService, IDisposable
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ILogger<BreweryService> _logger = logger;


    public async Task<List<Brewery>> GetBreweriesByCity(string city)
    {
        try
        {
            var url = $"https://api.openbrewerydb.org/v1/breweries?by_city={Uri.EscapeDataString(city)}&per_page=10";
            using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            if (response.IsSuccessStatusCode)
            {
                var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                var breweries = await JsonSerializer.DeserializeAsync<List<Brewery>>(stream).ConfigureAwait(false);
                return (breweries ?? new List<Brewery>());
            }
            else
            {
                return new List<Brewery>();
            }
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError("HTTP request error: {Error}", httpEx);
            return new List<Brewery>();
        }
        catch (Exception ex)
        {
            _logger.LogError("Error getting breweries: {Error}", ex);
            return new List<Brewery>();
        }
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
        GC.SuppressFinalize(this);
    }
}
