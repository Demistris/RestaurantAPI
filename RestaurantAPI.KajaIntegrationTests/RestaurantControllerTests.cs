using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Threading.Tasks;
using FluentAssertions;
using System.Net.Http;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace RestaurantAPI.KajaIntegrationTests
{
    public class RestaurantControllerTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private HttpClient _client;

        public RestaurantControllerTests(WebApplicationFactory<Startup> factory)
        {
            _client = factory.WithWebHostBuilder(builder => 
            { 
                builder.ConfigureServices(services => 
                {
                    var dbContextOptions = services.SingleOrDefault(service => service.ServiceType == typeof(DbContextOptions<RestaurantDbContext>));
                    services.Remove(dbContextOptions);

                    services.AddDbContext<RestaurantDbContext>(options => options.UseInMemoryDatabase("RestaurantDb"));
                }); 
            }).CreateClient();
        }

        [Theory]
        [InlineData("pageSize=5&pageNumber=1")]
        [InlineData("pageSize=15&pageNumber=2")]
        [InlineData("pageSize=10&pageNumber=3")]
        public async Task GetAll_WitchQueryParameters_ReturnsOkResult(string queryParams)
        {
            //Act
            var response = await _client.GetAsync("/api/restaurant?" + queryParams);

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
            //Act
            var response = await _client.GetAsync("/api/restaurant?" + queryParams);

            //Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }
    }
}
