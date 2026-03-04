using Rivr.Core.Models.Callbacks;
using Rivr.Core.Models.Orders;
using Rivr.Extensions;
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
    ""CreatedDate"": ""2024-08-24T07:59:43"",
    ""CompletedDate"": ""2024-08-24T08:59:43"",
    ""PaymentMethod"": ""Card""
}";

        // Act
        var orderCompleted = json.Deserialise<OrderCompleted>();

        // Assert
        orderCompleted.ShouldNotBeNull();
        orderCompleted.CreatedDate.ShouldBe(new DateTime(2024, 8, 24, 7, 59, 43));
        orderCompleted.CompletedDate.ShouldBe(new DateTime(2024, 8, 24, 8, 59, 43));
        orderCompleted.PaymentMethod.ShouldBe(PaymentMethod.Card);
    }

    [Test]
    public void ShouldDeserialiseStringWithMetadata()
    {
        var json = @"
{
    ""CreatedDate"": ""2024-08-24T07:59:43"",
    ""CompletedDate"": ""2024-08-24T08:59:43"",
    ""PaymentMethod"": ""Card"",
    ""Metadata"": { ""orderId"": ""12345"" }
}";

        var orderCompleted = json.Deserialise<OrderCompleted>();

        orderCompleted.ShouldNotBeNull();
        orderCompleted.Metadata.ShouldContainKeyAndValue("orderId", "12345");
    }

    [Test]
    public void ShouldDeserialiseCallbackWithObjectData()
    {
        var json = @"
{
    ""Id"": ""84c3c4ad-63d8-4d49-a4ff-ed57bbd40a39"",
    ""Type"": ""Order"",
    ""MerchantId"": ""c3073b9d-edd0-49f2-a28d-b7ded8ff9a8b"",
    ""Status"": ""Completed"",
    ""Data"": {
        ""CreatedDate"": ""2024-08-24T07:59:43"",
        ""CompletedDate"": ""2024-08-24T08:59:43"",
        ""PaymentMethod"": ""Card"",
        ""Metadata"": {}
    }
}";

        var callback = json.Deserialise<Callback>();

        callback.ShouldNotBeNull();
        callback.Id.ShouldBe(Guid.Parse("84c3c4ad-63d8-4d49-a4ff-ed57bbd40a39"));
        callback.MerchantId.ShouldBe(Guid.Parse("c3073b9d-edd0-49f2-a28d-b7ded8ff9a8b"));
        callback.Status.ShouldBe("Completed");
        callback.Data.ShouldNotBeNull();

        var orderCompleted = callback.Data.Deserialise<OrderCompleted>();
        orderCompleted.PaymentMethod.ShouldBe(PaymentMethod.Card);
    }

    [Test]
    public void ShouldDeserialiseCallbackWithStringData()
    {
        var dataJson = @"{""CreatedDate"":""2024-08-24T07:59:43"",""PaymentMethod"":""Swish""}";
        var json = $@"
{{
    ""Id"": ""84c3c4ad-63d8-4d49-a4ff-ed57bbd40a39"",
    ""Type"": ""Order"",
    ""MerchantId"": ""c3073b9d-edd0-49f2-a28d-b7ded8ff9a8b"",
    ""Status"": ""Completed"",
    ""Data"": ""{dataJson.Replace("\"", "\\\"")}""
}}";

        var callback = json.Deserialise<Callback>();

        callback.ShouldNotBeNull();
        callback.Data.ShouldNotBeNull();

        var orderCompleted = callback.Data.Deserialise<OrderCompleted>();
        orderCompleted.PaymentMethod.ShouldBe(PaymentMethod.Swish);
    }
}