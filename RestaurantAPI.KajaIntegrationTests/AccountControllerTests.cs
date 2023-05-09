using FluentAssertions;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RestaurantAPI.Entities;
using RestaurantAPI.KajaIntegrationTests.Helpers;
using RestaurantAPI.Models;
using RestaurantAPI.Services;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace RestaurantAPI.KajaIntegrationTests
{
    public class AccountControllerTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private HttpClient _client;
        private Mock<IAccountService> _accountServiceMock = new Mock<IAccountService>();

        public AccountControllerTests(WebApplicationFactory<Startup> factory)
        {
            _client = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var dbContextOptions = services.SingleOrDefault(service => service.ServiceType == typeof(DbContextOptions<RestaurantDbContext>));
                    services.Remove(dbContextOptions);

                    services.AddSingleton<IAccountService>(_accountServiceMock.Object);

                    services.AddDbContext<RestaurantDbContext>(options => options.UseInMemoryDatabase("RestaurantDb"));
                });
            }).CreateClient();
        }

        [Fact]
        public async Task Login_ForRegisteredUser_ReturnsOk()
        {
            //Arrange
            _accountServiceMock.Setup(e => e.GenerateJwt(It.IsAny<LoginDto>())).Returns("jwt");

            var loginDto = new LoginDto()
            {
                Email = "test@test.com",
                Password = "123456"
            };

            var httpContent = loginDto.ToJsonHttpContent();

            //Act
            var response = await _client.PostAsync("/api/account/login", httpContent);

            //Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task RegisterUser_ForValidModel_ReturnsOk()
        {
            //Arrange
            var registerUser = new RegisterUserDto()
            {
                Email = "test@test.com",
                Password = "123456",
                ConfirmPassword = "123456"
            };

            var httpContent = registerUser.ToJsonHttpContent();

            //Act
            var response = await _client.PostAsync("/api/account/register", httpContent);

            //Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }
        
        [Fact]
        public async Task RegisterUser_ForInvalidModel_ReturnsBadRequest()
        {
            //Arrange
            var registerUser = new RegisterUserDto()
            {
                Password = "123456",
                ConfirmPassword = "1234567"
            };

            var httpContent = registerUser.ToJsonHttpContent();

            //Act
            var response = await _client.PostAsync("/api/account/register", httpContent);

            //Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }
    }
}
