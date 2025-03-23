using BreweryFinder.API.Controllers;
using BreweryFinder.API.Models;
using BreweryFinder.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

public class BreweryFinderControllerTests
{
    private readonly Mock<ILogger<BreweryFinderController>> _mockLogger;
    private readonly Mock<IBreweryService> _mockBreweryService;
    private readonly Mock<IDistributedCache> _mockCache;
    private readonly BreweryFinderController _controller;

    public BreweryFinderControllerTests()
    {
        _mockLogger = new Mock<ILogger<BreweryFinderController>>();
        _mockBreweryService = new Mock<IBreweryService>();
        _mockCache = new Mock<IDistributedCache>();
        _controller = new BreweryFinderController(_mockLogger.Object, _mockBreweryService.Object, _mockCache.Object);
    }

    [Fact]
    public async Task GetBreweriesAsync_ReturnsCachedBreweries_WhenCacheIsNotEmpty()
    {
        // Arrange
        var searchCriteria = new BrewerySearchCriteria { City = "TestCity", State = "TestState" };
        var cacheKey = $"breweries_{searchCriteria.City}_{searchCriteria.State}_{searchCriteria.BreweryType}_{searchCriteria.Name}";
        var cachedBreweries = JsonSerializer.Serialize(new List<Brewery> { new Brewery { Name = "TestBrewery" } });
        _mockCache.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
          .ReturnsAsync(Encoding.UTF8.GetBytes(cachedBreweries));

        // Act
        var result = await _controller.GetBreweriesAsync(searchCriteria);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var breweries = Assert.IsType<List<Brewery>>(okResult.Value);
        Assert.Single(breweries);
        Assert.Equal("TestBrewery", breweries[0].Name);
    }

    [Fact]
    public async Task GetBreweriesAsync_ReturnsBreweriesFromService_WhenCacheIsEmpty()
    {
        // Arrange
        var searchCriteria = new BrewerySearchCriteria { City = "TestCity", State = "TestState" };
        var cacheKey = $"breweries_{searchCriteria.City}_{searchCriteria.State}_{searchCriteria.BreweryType}_{searchCriteria.Name}";
        _mockCache.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
         .ReturnsAsync((byte[])null);
        var breweriesFromService = new List<Brewery> { new Brewery { Name = "TestBrewery" } };
        _mockBreweryService.Setup(s => s.GetBreweriesAsync(searchCriteria)).ReturnsAsync(breweriesFromService);

        // Act
        var result = await _controller.GetBreweriesAsync(searchCriteria);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var breweries = Assert.IsType<List<Brewery>>(okResult.Value);
        Assert.Single(breweries);
        Assert.Equal("TestBrewery", breweries[0].Name);
     
    }

    [Fact]
    public async Task GetBreweriesAsync_ReturnsServiceUnavailable_WhenHttpRequestExceptionIsThrown()
    {
        // Arrange
        var searchCriteria = new BrewerySearchCriteria { City = "TestCity", State = "TestState" };
        _mockCache.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException());

        // Act
        var result = await _controller.GetBreweriesAsync(searchCriteria);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task GetBreweriesAsync_ReturnsInternalServerError_WhenExceptionIsThrown()
    {
        // Arrange
        var searchCriteria = new BrewerySearchCriteria { City = "TestCity", State = "TestState" };
        _mockCache.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new System.Exception());

        // Act
        var result = await _controller.GetBreweriesAsync(searchCriteria);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
    }
}
