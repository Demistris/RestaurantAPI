using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Threading.Tasks;
using FluentAssertions;
using System.Net.Http;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Entities;
using Microsoft.Extensions.DependencyInjection;
using RestaurantAPI.Models;
using Newtonsoft.Json;
using System.Text;
using Microsoft.AspNetCore.Authorization.Policy;
using RestaurantAPI.KajaIntegrationTests.Helpers;

namespace RestaurantAPI.KajaIntegrationTests
{
    public class RestaurantControllerTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private HttpClient _client;
        private WebApplicationFactory<Startup> _factory;

        public RestaurantControllerTests(WebApplicationFactory<Startup> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var dbContextOptions = services.SingleOrDefault(service => service.ServiceType == typeof(DbContextOptions<RestaurantDbContext>));
                    services.Remove(dbContextOptions);

                    services.AddSingleton<IPolicyEvaluator, FakePolicyEvaluator>();
                    services.AddMvc(option => option.Filters.Add(new FakeUserFilter()));

                    services.AddDbContext<RestaurantDbContext>(options => options.UseInMemoryDatabase("RestaurantDb"));
                });
            });

            _client = _factory.CreateClient();
        }

        private void SeedRestaurant(Restaurant restaurant)
        {
            var scopeFactory = _factory.Services.GetService<IServiceScopeFactory>();
            using var scope = scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetService<RestaurantDbContext>();

            dbContext.Restaurants.Add(restaurant);
            dbContext.SaveChanges();
        }

        [Fact]
        public async Task Delete_ForNonRestaurantOwner_ReturnsForbidden()
        {
            //Arrange
            var restaurant = new Restaurant()
            {
                CreatedById = 900,
            };

            //Seed
            SeedRestaurant(restaurant);

            //Act
            var response = await _client.DeleteAsync("/api/restaurant/" + restaurant.Id);

            //Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Delete_ForRestaurantOwner_ReturnsNoContent()
        {
            //Arrange
            var restaurant = new Restaurant()
            {
                CreatedById = 1,
                Name = "Test"
            };

            //Seed
            SeedRestaurant(restaurant);

            //Act
            var response = await _client.DeleteAsync("/api/restaurant/" + restaurant.Id);

            //Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Delete_ForNonExistingRestaurant_ReturnsNotFound()
        {
            //Act
            var response = await _client.DeleteAsync("/api/restaurant/987");

            //Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CreateRestaurant_WithValidModel_ReturnsCreatedStatus()
        {
            //Arrange
            var model = new CreateRestaurantDto()
            {
                Name = "TestRestaurant",
                City = "Kraków",
                Street = "Czysta 8"
            };

            var httpContent = model.ToJsonHttpContent();

            //Act
            var response = await _client.PostAsync("/api/restaurant", httpContent);

            //Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
            response.Headers.Location.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateRestaurant_WithInvalidModel_ReturnsBadRequest()
        {
            //Arrange
            var model = new CreateRestaurantDto()
            {
                ContactEmail = "test@test.com",
                Description = "Test description",
                ContactNumber = "999 888 777"
            };

            var httpContent = model.ToJsonHttpContent();

            //Act
            var response = await _client.PostAsync("/api/restaurant", httpContent);

            //Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
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
