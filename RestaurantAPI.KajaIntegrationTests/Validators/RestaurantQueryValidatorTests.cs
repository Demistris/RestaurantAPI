﻿using FluentValidation.TestHelper;
using RestaurantAPI.Entities;
using RestaurantAPI.Models;
using RestaurantAPI.Models.Validators;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace RestaurantAPI.KajaIntegrationTests.Validators
{
    public class RestaurantQueryValidatorTests
    {
        public static IEnumerable<object[]> GetSampleValidData()
        {
            var list = new List<RestaurantQuery>()
            {
                new RestaurantQuery()
                {
                    PageNumber = 1,
                    PageSize = 10
                },
                new RestaurantQuery()
                {
                    PageNumber = 2,
                    PageSize = 15
                },
                new RestaurantQuery()
                {
                    PageNumber = 22,
                    PageSize = 5,
                    SortBy = nameof(Restaurant.Name)
                },
                new RestaurantQuery()
                {
                    PageNumber = 12,
                    PageSize = 15,
                    SortBy = nameof(Restaurant.Category)
                }
            };

            return list.Select(q => new object[] { q });
        }

        public static IEnumerable<object[]> GetSampleInvalidData()
        {
            var list = new List<RestaurantQuery>()
            {
                new RestaurantQuery()
                {
                    PageNumber = 0,
                    PageSize = 10
                },
                new RestaurantQuery()
                {
                    PageNumber = 2,
                    PageSize = 13
                },
                new RestaurantQuery()
                {
                    PageNumber = 22,
                    PageSize = 5,
                    SortBy = nameof(Restaurant.ContactEmail)
                },
                new RestaurantQuery()
                {
                    PageNumber = 12,
                    PageSize = 15,
                    SortBy = nameof(Restaurant.ContactNumber)
                }
            };

            return list.Select(q => new object[] { q });
        }

        [Theory]
        [MemberData(nameof(GetSampleValidData))]
        public void Validate_ForCorrectModel_ReturnsSuccess(RestaurantQuery model)
        {
            //Arrange
            var validator = new RestaurantQueryValidator();

            //Act
            var result = validator.TestValidate(model);

            //Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [MemberData(nameof(GetSampleInvalidData))]
        public void Validate_ForIncorrectModel_ReturnsFailure(RestaurantQuery model)
        {
            //Arrange
            var validator = new RestaurantQueryValidator();

            //Act
            var result = validator.TestValidate(model);

            //Assert
            result.ShouldHaveAnyValidationError();
        }
    }
}
