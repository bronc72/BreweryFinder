using BreweryFinder.API.Models;
using BreweryFinder.API.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Text.Json;

namespace BreweryFinder.Tests
{
    public class BreweryServiceTests
    {
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly HttpClient _httpClient;
        private readonly BreweryService _breweryService;
        private readonly Mock<ILogger<BreweryService>> _logger;

        public BreweryServiceTests()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
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
            var result = await _breweryService.GetBreweriesByCity("flint");

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
            var result = await _breweryService.GetBreweriesByCity("flint");

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
            var result = await _breweryService.GetBreweriesByCity("flint");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}
