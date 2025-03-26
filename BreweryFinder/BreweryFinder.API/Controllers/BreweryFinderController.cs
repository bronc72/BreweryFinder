using BreweryFinder.API.Models;
using BreweryFinder.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace BreweryFinder.API.Controllers;

//[Authorize]
[ApiController]
[Route("api/[controller]")]
//[RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes")]
public class BreweryFinderController(ILogger<BreweryFinderController> logger, IBreweryService breweryService, IDistributedCache cache) : ControllerBase
{

    [HttpPost("search")]
    public async Task<IActionResult> GetBreweriesAsync([FromBody] BrewerySearchCriteria searchCriteria)
    {
        try
        {
            string cacheKey = $"breweries_{searchCriteria.City}_{searchCriteria.State}_{searchCriteria.BreweryType}_{searchCriteria.Name}";
            var cachedBreweries = await cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedBreweries))
            {
                return ReturnCachedBreweries(cachedBreweries);
            }
            else
            {
                return await CacheAndReturnBreweriesAsync(cacheKey, await breweryService.GetBreweriesAsync(searchCriteria));
            }
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


    [HttpGet("byCity")]
    public async Task<IActionResult> GetBreweriesByCityAsync(string city)
    {
        try
        {
            string cacheKey = $"breweries_byCity_{city}";
            var cachedBreweries = await cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedBreweries))
            {
                return ReturnCachedBreweries(cachedBreweries);
            }
            else
            {
                return await CacheAndReturnBreweriesAsync(cacheKey, await breweryService.GetBreweriesByCityAsync(city));
            }
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

    [HttpGet("byState")]
    public async Task<IActionResult> GetBreweriesByState(string state)
    {
        try
        {
            string cacheKey = $"breweries_byState_{state}";
            var cachedBreweries = await cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedBreweries))
            {
                return ReturnCachedBreweries(cachedBreweries);
            }
            else
            {
                return await CacheAndReturnBreweriesAsync(cacheKey, await breweryService.GetBreweriesByStateAsync(state));
            }
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

    [HttpGet("byType")]
    public async Task<IActionResult> GetBreweriesByType(string type)
    {
        try
        {
            string cacheKey = $"breweries_byType_{type}";
            var cachedBreweries = await cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedBreweries))
            {
                return ReturnCachedBreweries(cachedBreweries);
            }
            else
            {
                return await CacheAndReturnBreweriesAsync(cacheKey, await breweryService.GetBreweriesByTypeAsync(type));
            }
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

    private OkObjectResult ReturnCachedBreweries(string cachedBreweries)
    {
        var breweries = JsonSerializer.Deserialize<List<Brewery>>(cachedBreweries);
        return Ok(breweries);
    }

    private async Task<IActionResult> CacheAndReturnBreweriesAsync(string cacheKey, List<Brewery>? breweries)
    {
        if (breweries != null)
        {
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
            };
            await cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(breweries), cacheOptions);
        }

        return Ok(breweries ?? new List<Brewery>());
    }
}
