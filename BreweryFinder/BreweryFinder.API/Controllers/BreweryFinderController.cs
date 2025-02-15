using BreweryFinder.API.Models;
using BreweryFinder.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

namespace BreweryFinder.API.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
[RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes")]
public class BreweryFinderController(ILogger<BreweryFinderController> logger, IBreweryService breweryService) : ControllerBase
{
    private readonly ILogger<BreweryFinderController> _logger = logger;
    private readonly IHttpClientFactory httpClientFactory;
    private readonly IBreweryService _breweryService = breweryService;


    [HttpGet(Name = "GetBreweries")]
    public async Task<IEnumerable<Brewery>> GetAsync()
    {
        using HttpClient client = httpClientFactory.CreateClient();
        try
        {
            var state = "michigan";
            var url = $"/v1/breweries?by_state={state}";
            using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            var stream = await response.Content.ReadAsStreamAsync();
            var breweries = await JsonSerializer.DeserializeAsync<List<Brewery>>(stream);
            return breweries ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError("Error getting something fun to say: {Error}", ex);
        }

        return [];

    }
}
