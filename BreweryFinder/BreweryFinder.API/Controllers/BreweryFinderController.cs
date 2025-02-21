using BreweryFinder.API.Models;
using BreweryFinder.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace BreweryFinder.API.Controllers;

//[Authorize]
[ApiController]
[Route("[controller]")]
//[RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes")]
public class BreweryFinderController : ControllerBase
{
    private readonly ILogger<BreweryFinderController> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IBreweryService _breweryService;
    private readonly IDistributedCache _cache;

    public BreweryFinderController(ILogger<BreweryFinderController> logger, IHttpClientFactory httpClientFactory, IBreweryService breweryService, IDistributedCache cache)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _breweryService = breweryService;
        _cache = cache;
    }

    [HttpGet("GetBreweriesAsync")]
    public async Task<IActionResult> GetAsync(string city)
    {
        string cacheKey = $"breweries_byCity{city}";
        var cachedBreweries = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedBreweries))
        {
            var breweries = JsonSerializer.Deserialize<List<Brewery>>(cachedBreweries);
            return Ok(breweries);
        }

        try
        {
            _logger.LogInformation("Getting breweries for city: {City}", city);
            var breweries = await _breweryService.GetBreweriesByCity(city);

            if (breweries != null)
            {
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
                };
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(breweries), cacheOptions);
            }

            return Ok(breweries ?? []);
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError("HTTP request error: {Error}", httpEx);
            return StatusCode(StatusCodes.Status503ServiceUnavailable, "Service unavailable. Please try again later.");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error getting breweries: {Error}", ex);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
        }
    }
}
