using Rivr.Extensions;
using Rivr.Models.Callbacks;
using Rivr.Models.Orders;
using Shouldly;

namespace Rivr.Test;

public class StringExtensionTests
{
    [Test]
    public void ShouldDeserialiseString()
    {
        // Arrange
        var json = @"
{
    ""CreatedDate"": ""2024-08-24T07:59:43.6130000+02:00"",
    ""CompletedDate"": ""2024-08-24T08:59:43.6130000+02:00"",
    ""PaymentMethod"": ""Card""
}";

        // Act
        var orderCompleted = json.Deserialise<OrderCompleted>();

        // Assert
        orderCompleted.ShouldNotBeNull();
        orderCompleted.CreatedDate.ShouldBe(new DateTime(2024, 8, 24, 7, 59, 43, 613));
        orderCompleted.CompletedDate.ShouldBe(new DateTime(2024, 8, 24, 8, 59, 43, 613));
        orderCompleted.PaymentMethod.ShouldBe(PaymentMethod.Card);
    }
}