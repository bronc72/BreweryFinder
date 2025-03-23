using BreweryFinder.API.Models;
using BreweryFinder.API.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Text.Json;

namespace Test.ApiTests;

public class BreweryServiceTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly BreweryService _breweryService;
    private readonly Mock<ILogger<BreweryService>> _logger;

    public BreweryServiceTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://fake.fake")
        };
        _logger = new Mock<ILogger<BreweryService>>();
        _breweryService = new BreweryService(_httpClient, _logger.Object);
    }

    [Fact]
    public async Task GetBreweriesByCity_ReturnsListOfBreweries()
    {
        // Arrange
        var breweries = new List<Brewery>
            {
                new Brewery { Name = "Brewery1", City = "Flint", State = "Michigan" },
                new Brewery { Name = "Brewery2", City = "Flint", State = "Michigan" }
            };
        var responseContent = JsonSerializer.Serialize(breweries);
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent)
            });

        // Act
        var result = await _breweryService.GetBreweriesByCityAsync("flint");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("Brewery1", result[0].Name);
        Assert.Equal("Brewery2", result[1].Name);
    }

    [Fact]
    public async Task GetBreweriesByCity_ReturnsEmptyList_OnHttpRequestException()
    {
        // Arrange
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new HttpRequestException());

        // Act
        var result = await _breweryService.GetBreweriesByCityAsync("flint");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetBreweriesByCity_ReturnsEmptyList_OnNonSuccessStatusCode()
    {
        // Arrange
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        // Act
        var result = await _breweryService.GetBreweriesByCityAsync("flint");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }


    [Fact]
    public async Task GetBreweriesByStateAsync_ReturnsBreweries_WhenResponseIsSuccessful()
    {
        // Arrange
        var state = "California";
        var responseContent = "[{\"name\":\"Brewery1\",\"brewery_type\":\"micro\",\"street\":\"123 Main St\",\"city\":\"San Diego\",\"state\":\"California\",\"postal_code\":\"92101\",\"phone\":\"1234567890\",\"website_url\":\"http://brewery1.com\"}]";
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent)
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(responseMessage);

        // Act
        var result = await _breweryService.GetBreweriesByStateAsync(state);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Brewery1", result[0].Name);
    }

    [Fact]
    public async Task GetBreweriesByStateAsync_ReturnsEmptyList_WhenResponseIsUnsuccessful()
    {
        // Arrange
        var state = "California";
        var responseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(responseMessage);

        // Act
        var result = await _breweryService.GetBreweriesByStateAsync(state);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetBreweriesByTypeAsync_ReturnsBreweries_WhenResponseIsSuccessful()
    {
        // Arrange
        var responseContent = "[{\"name\":\"Brewery1\",\"brewery_type\":\"micro\",\"street\":\"123 Main St\",\"city\":\"City1\",\"state\":\"State1\",\"postal_code\":\"12345\",\"phone\":\"123-456-7890\",\"website_url\":\"http://brewery1.com\"}]";
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent)
            });

        // Act
        var result = await _breweryService.GetBreweriesByTypeAsync("micro");

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Brewery1", result[0].Name);
    }

    [Fact]
    public async Task GetBreweriesByTypeAsync_ReturnsEmptyList_WhenResponseIsUnsuccessful()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            });

        // Act
        var result = await _breweryService.GetBreweriesByTypeAsync("micro");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }



    [Fact]
    public async Task GetBreweriesAsync_WithValidSearchCriteria_ReturnsBreweries()
    {
        // Arrange
        var searchCriteria = new BrewerySearchCriteria { City = "San Diego", State = "California", BreweryType = "micro" };
        var responseContent = "[{\"name\":\"Brewery 1\",\"brewery_type\":\"micro\",\"city\":\"San Diego\",\"state\":\"California\"}]";
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent)
        };


        _httpMessageHandlerMock.Protected()
                               .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                               .ReturnsAsync(responseMessage);
        // Act
        var result = await _breweryService.GetBreweriesAsync(searchCriteria);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Brewery 1", result[0].Name);
    }

    [Fact]
    public async Task GetBreweriesAsync_WithNullSearchCriteria_ReturnsEmptyList()
    {
        // Act
        var result = await _breweryService.GetBreweriesAsync(null);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);

    }
}
