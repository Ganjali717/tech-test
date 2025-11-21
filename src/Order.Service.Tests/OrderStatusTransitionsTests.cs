using NUnit.Framework;
using Order.Service.Exceptions;
using Order.Service.Status;

namespace Order.Service.Tests
{
    public class OrderStatusTransitionsTests
    {
        [Test]
        public void Valid_transition_does_not_throw()
        {
            Assert.DoesNotThrow(() =>
                OrderStatusTransitions.Validate("Created", "In Progress"));
        }

        [Test]
        public void Invalid_transition_throws()
        {
            var ex = Assert.Throws<InvalidOrderStatusTransitionException>(() =>
                OrderStatusTransitions.Validate("Completed", "In Progress"));

            StringAssert.Contains("Cannot change status", ex.Message);
        }

        [Test]
        public void Unknown_status_throws()
        {
            Assert.Throws<InvalidOrderStatusTransitionException>(() =>
                OrderStatusTransitions.Validate("Something", "In Progress"));
        }
    }
}