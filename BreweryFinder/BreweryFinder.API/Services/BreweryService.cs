using BreweryFinder.API.Models;
using System.Text.Json;

namespace BreweryFinder.API.Services;

public class BreweryService : IBreweryService, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BreweryService> _logger;
    private bool _disposed;

    public BreweryService(HttpClient httpClient, ILogger<BreweryService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _httpClient.BaseAddress = new Uri("https://api.openbrewerydb.org/v1/breweries"); // Set the base address here
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<Brewery>> GetBreweriesAsync(BrewerySearchCriteria searchCriteria)
    {
        if (searchCriteria != null)
        {
            string url = BuildSearchUrl(searchCriteria);

            return await GetBreweriesInternalAsync(url);
        }
        else
        {
            return new List<Brewery>();
        }
    }

    private string BuildSearchUrl(BrewerySearchCriteria searchCriteria)
    {
        var url = $"{_httpClient.BaseAddress}?";

        if (!string.IsNullOrEmpty(searchCriteria.City))
        {
            url += $"by_city={Uri.EscapeDataString(searchCriteria.City)}&";
        }
        if (!string.IsNullOrEmpty(searchCriteria.State))
        {
            url += $"by_state={Uri.EscapeDataString(searchCriteria.State)}&";
        }
        if (!string.IsNullOrEmpty(searchCriteria.BreweryType))
        {
            url += $"by_type={Uri.EscapeDataString(searchCriteria.BreweryType)}&";
        }
        if (!string.IsNullOrEmpty(searchCriteria.Name))
        {
            url += $"by_name={Uri.EscapeDataString(searchCriteria.Name)}&";
        }
        if (!string.IsNullOrEmpty(searchCriteria.PostalCode))
        {
            url += $"by_postalcode={Uri.EscapeDataString(searchCriteria.PostalCode)}&";
        }
     
        return url.TrimEnd('&');
    }

    public async Task<List<Brewery>> GetBreweriesByCityAsync(string city)
    {
      
        var url = $"{_httpClient.BaseAddress}?by_city={Uri.EscapeDataString(city)}";
        return await GetBreweriesInternalAsync(url).ConfigureAwait(false);
    }

    public async Task<List<Brewery>> GetBreweriesByStateAsync(string state)
    {

        var url = $"{_httpClient.BaseAddress}?by_type={Uri.EscapeDataString(state)}";
        return await GetBreweriesInternalAsync(url).ConfigureAwait(false);
    }


    public async Task<List<Brewery>> GetBreweriesByTypeAsync(string type)
    {
        var url = $"{_httpClient.BaseAddress}?by_type={Uri.EscapeDataString(type)}";
        return await GetBreweriesInternalAsync(url).ConfigureAwait(false);
    }

    private async Task<List<Brewery>> GetBreweriesInternalAsync(string url)
    {
        try
        {

            using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

            return await ParseBreweriesFromResponseAsync(response);
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "HTTP request error: {Error}", httpEx.Message);
            return new List<Brewery>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting breweries by state: {Error}", ex.Message);
            return new List<Brewery>();
        }
    }

    private static async Task<List<Brewery>> ParseBreweriesFromResponseAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            var breweries = await JsonSerializer.DeserializeAsync<List<Brewery>>(stream);
            return breweries ?? new List<Brewery>();
        }
        else
        {
            return new List<Brewery>();
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _httpClient?.Dispose();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }


}
