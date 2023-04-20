using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Threading.Tasks;
using FluentAssertions;

namespace RestaurantAPI.KajaIntegrationTests
{
    public class RestaurantControllerTests
    {
        [Theory]
        [InlineData("pageSize=5&pageNumber=1")]
        [InlineData("pageSize=15&pageNumber=2")]
        [InlineData("pageSize=10&pageNumber=3")]
        public async Task GetAll_WitchQueryParameters_ReturnsOkResult(string queryParams)
        {
            //Arrange
            var factory = new WebApplicationFactory<Startup>();
            var client = factory.CreateClient();

            //Act
            var response = await client.GetAsync("/api/restaurant?" + queryParams);

            //Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Theory]
        [InlineData("pageSize=100&pageNumber=3")]
        [InlineData("pageSize=11&pageNumber=3")]
        [InlineData(null)]
        [InlineData("")]
        public async Task GetAll_WithInvalidQueryParams_ReturnsBadRequest(string queryParams)
        {
            //Arrange
            var factory = new WebApplicationFactory<Startup>();
            var client = factory.CreateClient();

            //Act
            var response = await client.GetAsync("/api/restaurant?" + queryParams);

            //Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }
    }
}
