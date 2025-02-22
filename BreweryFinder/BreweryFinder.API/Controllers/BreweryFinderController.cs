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
public class BreweryFinderController(ILogger<BreweryFinderController> logger, IBreweryService breweryService, IDistributedCache cache) : ControllerBase
{
    [HttpGet("GetBreweriesAsync")]
    public async Task<IActionResult> GetAsync(string city)
    {
        string cacheKey = "breweries_byCity_flint";
        var cachedBreweries = await cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedBreweries))
        {
            var breweries = JsonSerializer.Deserialize<List<Brewery>>(cachedBreweries);
            return Ok(breweries);
        }

        try
        {
            logger.LogInformation("Getting breweries for city: {City}", city);
            var breweries = await breweryService.GetBreweriesByCity(city);

            if (breweries != null)
                logger.LogInformation("Getting breweries for city: {City}", city);
            {
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
                };
                await cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(breweries), cacheOptions);
            }

            return Ok(breweries ?? []);
        }
        catch (HttpRequestException httpEx)
        {
            logger.LogError(httpEx, "HTTP request error: {Error}", httpEx.Message);
            return StatusCode(StatusCodes.Status503ServiceUnavailable, "Service unavailable. Please try again later.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting breweries: {Error}", ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
        }
    }
}
