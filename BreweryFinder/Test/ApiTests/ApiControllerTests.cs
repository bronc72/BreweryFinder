using BreweryFinder.API.Controllers;
using BreweryFinder.API.Models;
using BreweryFinder.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Text.Json;
using Xunit;


namespace BreweryFinder.Tests
{
    public class BreweryFinderControllerTests
    {
        private readonly Mock<ILogger<BreweryFinderController>> _loggerMock;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<IBreweryService> _breweryServiceMock;
        private readonly BreweryFinderController _controller;

        public BreweryFinderControllerTests()
        {
            _loggerMock = new Mock<ILogger<BreweryFinderController>>();
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _breweryServiceMock = new Mock<IBreweryService>();
            _controller = new BreweryFinderController(_loggerMock.Object, _httpClientFactoryMock.Object, _breweryServiceMock.Object);
        }

        [Fact]
        public async Task GetAsync_ReturnsOkResult_WithListOfBreweries()
        {
            // Arrange
            var breweries = new List<Brewery>
                {
                    new Brewery { Name = "Brewery1", City = "Flint", State = "Michigan" },
                    new Brewery { Name = "Brewery2", City = "Flint", State = "Michigan" }
                };

            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(breweries))
                });

            var httpClient = new HttpClient(httpMessageHandlerMock.Object);
            _httpClientFactoryMock.Setup(factory => factory.CreateClient(It.IsAny<string>())).Returns(httpClient);

            // Act
            var result = await _controller.GetAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnBreweries = Assert.IsType<List<Brewery>>(okResult.Value);
            Assert.Equal(2, returnBreweries.Count);
        }

        [Fact]
        public async Task GetAsync_ReturnsServiceUnavailable_OnHttpRequestException()
        {
            // Arrange
            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new HttpRequestException());

            var httpClient = new HttpClient(httpMessageHandlerMock.Object);
            _httpClientFactoryMock.Setup(factory => factory.CreateClient(It.IsAny<string>())).Returns(httpClient);

            // Act
            var result = await _controller.GetAsync();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.ServiceUnavailable, statusCodeResult.StatusCode);
        }
    }
}
