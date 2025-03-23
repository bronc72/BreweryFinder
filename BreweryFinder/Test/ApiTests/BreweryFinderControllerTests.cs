using BreweryFinder.API.Controllers;
using BreweryFinder.API.Models;
using BreweryFinder.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;
using System.Text.Json;

namespace BreweryFinder.Tests.ApiTests;

public class BreweryFinderControllerTests
{
    private readonly Mock<ILogger<BreweryFinderController>> _loggerMock;
    private readonly Mock<IBreweryService> _breweryServiceMock;
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly BreweryFinderController _controller;

    public BreweryFinderControllerTests()
    {
        _loggerMock = new Mock<ILogger<BreweryFinderController>>();
        _breweryServiceMock = new Mock<IBreweryService>();
        _cacheMock = new Mock<IDistributedCache>();
        _controller = new BreweryFinderController(_loggerMock.Object, _breweryServiceMock.Object, _cacheMock.Object);
    }

    [Fact]
    public async Task GetBreweriesByCityAsync_ReturnsCachedBreweries_WhenCacheIsNotEmpty()
    {
        // Arrange
        string city = "TestCity";
        string cacheKey = $"breweries_byCity_{city}";
        var breweries = new List<Brewery> { new Brewery { Name = "Test Brewery" } };
        string cachedBreweries = JsonSerializer.Serialize(breweries);

        _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(Encoding.UTF8.GetBytes(cachedBreweries));

        // Act
        var result = await _controller.GetBreweriesByCityAsync(city);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedBreweries = Assert.IsType<List<Brewery>>(okResult.Value);
        Assert.Single(returnedBreweries);
        Assert.Equal("Test Brewery", returnedBreweries[0].Name);
    }

    [Fact]
    public async Task GetBreweriesByCityAsync_ReturnsBreweriesFromService_WhenCacheIsEmpty()
    {
        // Arrange
        string city = "TestCity";
        string cacheKey = $"breweries_byCity_{city}";
        var breweries = new List<Brewery> { new Brewery { Name = "Test Brewery" } };
        var cachedBreweries = JsonSerializer.Serialize(breweries);
        _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
          .ReturnsAsync(Encoding.UTF8.GetBytes(cachedBreweries));
        _breweryServiceMock.Setup(s => s.GetBreweriesByCityAsync(city)).ReturnsAsync(breweries);

        // Act
        var result = await _controller.GetBreweriesByCityAsync(city);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedBreweries = Assert.IsType<List<Brewery>>(okResult.Value);
        Assert.Single(returnedBreweries);
        Assert.Equal("Test Brewery", returnedBreweries[0].Name);
    }

    [Fact]
    public async Task GetBreweriesByState_ReturnsCachedBreweries_WhenCacheIsNotEmpty()
    {
        // Arrange
        string state = "TestState";
        string cacheKey = $"breweries_byState_{state}";
        var breweries = new List<Brewery> { new Brewery { Name = "Test Brewery" } };
        string cachedBreweries = JsonSerializer.Serialize(breweries);
        _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync(Encoding.UTF8.GetBytes(cachedBreweries));

        // Act
        var result = await _controller.GetBreweriesByState(state);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedBreweries = Assert.IsType<List<Brewery>>(okResult.Value);
        Assert.Single(returnedBreweries);
        Assert.Equal("Test Brewery", returnedBreweries[0].Name);
    }

    [Fact]
    public async Task GetBreweriesByState_ReturnsBreweriesFromService_WhenCacheIsEmpty()
    {
        // Arrange
        string state = "TestState";
        string cacheKey = $"breweries_byState_{state}";
        var breweries = new List<Brewery> { new Brewery { Name = "Test Brewery" } };

        _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
          .ReturnsAsync((byte[])null);
        _breweryServiceMock.Setup(s => s.GetBreweriesByStateAsync(state)).ReturnsAsync(breweries);

        // Act
        var result = await _controller.GetBreweriesByState(state);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedBreweries = Assert.IsType<List<Brewery>>(okResult.Value);
        Assert.Single(returnedBreweries);
        Assert.Equal("Test Brewery", returnedBreweries[0].Name);
    }

    [Fact]
    public async Task GetBreweriesByType_ReturnsCachedBreweries_WhenCacheIsNotEmpty()
    {
        // Arrange
        string type = "TestType";
        string cacheKey = $"breweries_byType_{type}";
        var breweries = new List<Brewery> { new Brewery { Name = "Test Brewery" } };
        string cachedBreweries = JsonSerializer.Serialize(breweries);
        _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync(Encoding.UTF8.GetBytes(cachedBreweries));

        // Act
        var result = await _controller.GetBreweriesByType(type);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedBreweries = Assert.IsType<List<Brewery>>(okResult.Value);
        Assert.Single(returnedBreweries);
        Assert.Equal("Test Brewery", returnedBreweries[0].Name);
    }

    [Fact]
    public async Task GetBreweriesByType_ReturnsBreweriesFromService_WhenCacheIsEmpty()
    {
        // Arrange
        string type = "TestType";
        string cacheKey = $"breweries_byType_{type}";
        var breweries = new List<Brewery> { new Brewery { Name = "Test Brewery" } };
        _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
         .ReturnsAsync((byte[])null); 
        _breweryServiceMock.Setup(s => s.GetBreweriesByTypeAsync(type)).ReturnsAsync(breweries);

        // Act
        var result = await _controller.GetBreweriesByType(type);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedBreweries = Assert.IsType<List<Brewery>>(okResult.Value);
        Assert.Single(returnedBreweries);
        Assert.Equal("Test Brewery", returnedBreweries[0].Name);
    }
}
