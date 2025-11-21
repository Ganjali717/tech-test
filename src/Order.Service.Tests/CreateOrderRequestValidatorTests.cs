using System;
using System.Collections.Generic;
using FluentValidation.TestHelper;
using NUnit.Framework;
using Order.Model.Requests;
using OrderService.WebAPI.Validation;

namespace Order.Service.Tests
{
    public class CreateOrderRequestValidatorTests
    {
        private CreateOrderRequestValidator _validator;

        [SetUp]
        public void SetUp()
        {
            _validator = new CreateOrderRequestValidator();
        }

        [Test]
        public void Empty_items_should_have_error()
        {
            var model = new CreateOrderRequest
            {
                ResellerId = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                Items = new List<CreateOrderItemRequest>()
            };

            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Items);
        }

        [Test]
        public void Non_positive_quantity_has_error()
        {
            var model = new CreateOrderRequest
            {
                ResellerId = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                Items = new List<CreateOrderItemRequest>
                {
                    new()
                    {
                        ProductId = Guid.NewGuid(),
                        Quantity = 0
                    }
                }
            };

            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor("Items[0].Quantity");
        }

        [Test]
        public void Valid_request_has_no_errors()
        {
            var model = new CreateOrderRequest
            {
                ResellerId = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                Items = new List<CreateOrderItemRequest>
                {
                    new()
                    {
                        ProductId = Guid.NewGuid(),
                        Quantity = 2
                    }
                }
            };

            var result = _validator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
