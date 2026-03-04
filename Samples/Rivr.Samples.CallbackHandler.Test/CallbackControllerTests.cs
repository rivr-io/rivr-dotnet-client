using System.Net.Http.Json;
using System.Text.Json;
using Rivr.Core.Models.Callbacks;
using Rivr.Core.Models.Orders;
using OrderStatus = Rivr.Core.Models.Orders.OrderStatus;

namespace Rivr.Samples.CallbackHandler.Test
{
    public class CallbackControllerTests
    {
        protected HttpClient Client = null!;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Client = TestableWebApplication<Program>
                .CreateClient(services => { });
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Client.Dispose();
        }

        [Test]
        public async Task ShouldPerformCallbackForCompleted()
        {
            var response = await Client.PostAsJsonAsync("/callback", new
            {
                Id = Guid.NewGuid(),
                Type = "Order",
                MerchantId = Guid.NewGuid(),
                Status = "Completed",
                Data = new
                {
                    CreatedDate = DateTime.Now.AddHours(-1),
                    CompletedDate = DateTime.Now,
                    PaymentMethod = "Card",
                    Metadata = new Dictionary<string, string> { { "key1", "value1" } }
                }
            });

            response.EnsureSuccessStatusCode();
        }

        [Test]
        public async Task ShouldPerformCallbackForPending()
        {
            var response = await Client.PostAsJsonAsync("/callback", new
            {
                Id = Guid.NewGuid(),
                Type = "Order",
                MerchantId = Guid.NewGuid(),
                Status = "Pending",
                Data = new
                {
                    CreatedDate = DateTime.Now.AddHours(-1),
                    Metadata = new Dictionary<string, string>()
                }
            });

            response.EnsureSuccessStatusCode();
        }

        [Test]
        public async Task ShouldPerformCallbackForCompletedWithStringData()
        {
            var response = await Client.PostAsJsonAsync("/callback", new Callback
            {
                Id = Guid.NewGuid(),
                Type = CallbackType.Order,
                MerchantId = Guid.NewGuid(),
                Status = OrderStatus.Completed.ToString(),
                Data = JsonSerializer.Serialize(new
                {
                    CreatedDate = DateTime.Now.AddHours(-1),
                    CompletedDate = DateTime.Now,
                    PaymentMethod = PaymentMethod.Card.ToString()
                })
            });

            response.EnsureSuccessStatusCode();
        }
    }
}