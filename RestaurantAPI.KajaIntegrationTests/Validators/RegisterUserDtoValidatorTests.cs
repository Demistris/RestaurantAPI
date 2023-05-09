using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Entities;
using RestaurantAPI.Models;
using RestaurantAPI.Models.Validators;
using System.Collections.Generic;
using Xunit;

namespace RestaurantAPI.KajaIntegrationTests.Validators
{
    public class RegisterUserDtoValidatorTests
    {
        private readonly RestaurantDbContext _dbContext;

        public RegisterUserDtoValidatorTests()
        {
            var builder = new DbContextOptionsBuilder<RestaurantDbContext>();
            builder.UseInMemoryDatabase("TestDb");

            _dbContext = new RestaurantDbContext(builder.Options);
            Seed();
        }

        private void Seed()
        {
            var testUsers = new List<User>()
            {
                new User()
                {
                    Email = "test1@test.com"
                },
                new User()
                {
                    Email = "test2@test.com"
                }
            };

            _dbContext.Users.AddRange(testUsers);
            _dbContext.SaveChanges();
        }

        [Fact]
        public void Validate_ForValidModel_ReturnsSuccess()
        {
            //Arrange
            var model = new RegisterUserDto()
            {
                Email = "test@test.com",
                Password = "123456",
                ConfirmPassword = "123456"
            };

            var validator = new RegisterUserDtoValidator(_dbContext);

            //Act
            var result = validator.TestValidate(model);

            //Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_ForInvalidModel_ReturnsFailure()
        {
            //Arrange
            var model = new RegisterUserDto()
            {
                Email = "test1@test.com",
                Password = "123456",
                ConfirmPassword = "123456"
            };

            var validator = new RegisterUserDtoValidator(_dbContext);

            //Act
            var result = validator.TestValidate(model);

            //Assert
            result.ShouldHaveAnyValidationError();
        }
    }
}
