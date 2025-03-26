using BreweryFinder.API.Models;
using BreweryFinder.API.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Text.Json;

namespace BreweryFinder.Tests.ApiTests;

public class BreweryServiceTests
{
    private readonly Mock<ILogger<BreweryService>> _loggerMock;
    private readonly Mock<HttpMessageHandler> _handlerMock;
    private readonly BreweryService _service;

    public BreweryServiceTests()
    {
        _loggerMock = new Mock<ILogger<BreweryService>>();
        _handlerMock = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(_handlerMock.Object);
        _service = new BreweryService(httpClient, _loggerMock.Object);
    }

    [Theory]
    [InlineData("Denver", null, null, null, null)]
    [InlineData(null, "Colorado", null, null, null)]
    [InlineData(null, null, "micro", null, null)]
    [InlineData("Denver", "Colorado", "micro", "Test Brewery", "80202")]
    public async Task GetBreweriesAsync_WithValidCriteria_ReturnsBreweries(
        string city, string state, string type, string name, string postalCode)
    {
        // Arrange
        var searchCriteria = new BrewerySearchCriteria
        {
            City = city,
            State = state,
            BreweryType = type,
            Name = name,
            PostalCode = postalCode
        };

        var expectedBreweries = new List<Brewery>
        {
            new()
            {
                Name = "Test Brewery",
                City = city,
                State = state,
                BreweryType = type,
                PostalCode = postalCode
            }
        };

        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(expectedBreweries))
        };

        _handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act
        var result = await _service.GetBreweriesAsync(searchCriteria);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        var brewery = result[0];
        Assert.Equal("Test Brewery", brewery.Name);
    }

    [Fact]
    public async Task GetBreweriesAsync_WithNullCriteria_ReturnsEmptyList()
    {
        // Act
        var result = await _service.GetBreweriesAsync(null);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task GetBreweriesAsync_WithErrorResponse_ReturnsEmptyList(HttpStatusCode statusCode)
    {
        // Arrange
        var searchCriteria = new BrewerySearchCriteria { City = "InvalidCity" };

        var response = new HttpResponseMessage
        {
            StatusCode = statusCode,
            Content = new StringContent("")
        };

        _handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act
        var result = await _service.GetBreweriesAsync(searchCriteria);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}
