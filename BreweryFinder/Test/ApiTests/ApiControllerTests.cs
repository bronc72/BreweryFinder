using BreweryFinder.API.Controllers;
using BreweryFinder.API.Models;
using BreweryFinder.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;
using System.Text.Json;

namespace Test.ApiTests
{
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
            _controller = new BreweryFinderController(_loggerMock.Object,  _breweryServiceMock.Object, _cacheMock.Object);
        }

        [Fact]
        public async Task GetAsync_ReturnsOkResult_WithListOfBreweries_FromCache()
        {
            // Arrange
            var breweries = new List<Brewery>
            {
                new() { Name = "Brewery1", City = "Flint", State = "Michigan" },
                new() { Name = "Brewery2", City = "Flint", State = "Michigan" }
            };
            var cachedBreweries = JsonSerializer.Serialize(breweries);

            _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Encoding.UTF8.GetBytes(cachedBreweries));

            // Act
            var result = await _controller.GetAsync("flint");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnBreweries = Assert.IsType<List<Brewery>>(okResult.Value);
            Assert.Equal(2, returnBreweries.Count);
        }

        [Fact]
        public async Task GetAsync_ReturnsOkResult_WithListOfBreweries_FromService()
        {
            // Arrange
            var breweries = new List<Brewery>
            {
                new() { Name = "Brewery1", City = "Flint", State = "Michigan" },
                new() { Name = "Brewery2", City = "Flint", State = "Michigan" }
            };


            _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((byte[])null);

            _breweryServiceMock.Setup(service => service.GetBreweriesByCity(It.IsAny<string>())).ReturnsAsync(breweries);

            // Act
            var result = await _controller.GetAsync("flint");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnBreweries = Assert.IsType<List<Brewery>>(okResult.Value);
            Assert.Equal(2, returnBreweries.Count);
        }

        [Fact]
        public async Task GetAsync_ReturnsServiceUnavailable_OnHttpRequestException()
        {
            // Arrange
            _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((byte[])null);

            _breweryServiceMock.Setup(service => service.GetBreweriesByCity(It.IsAny<string>())).ThrowsAsync(new HttpRequestException());

            // Act
            var result = await _controller.GetAsync("flint");

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(503, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task GetAsync_ReturnsInternalServerError_OnException()
        {
            // Arrange
            _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((byte[])null);

            _breweryServiceMock.Setup(service => service.GetBreweriesByCity(It.IsAny<string>())).ThrowsAsync(new Exception());

            // Act
            var result = await _controller.GetAsync("flint");

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }
    }
}
