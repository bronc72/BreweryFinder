using BreweryFinder.API.Models;
using BreweryFinder.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using System.Net.Http;
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

    public BreweryFinderController(ILogger<BreweryFinderController> logger, IHttpClientFactory httpClientFactory, IBreweryService breweryService)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _breweryService = breweryService;
    }

    [HttpGet("GetBreweriesAsync")]
    public async Task<IActionResult> GetAsync()
    {
        using HttpClient client = _httpClientFactory.CreateClient();
        try
        {
            var state = "michigan";
            var url = $"https://api.openbrewerydb.org/v1/breweries?by_city=flint&per_page=10";
            using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            var breweries = await JsonSerializer.DeserializeAsync<List<Brewery>>(stream).ConfigureAwait(false);
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
