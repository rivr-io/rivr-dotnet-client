using System.Net.Http.Json;
using System.Text.Json;
using Rivr.Models.Callbacks;
using Rivr.Models.Orders;
using OrderStatus = Rivr.Models.Orders.OrderStatus;

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
        public async Task ShouldPerformCallback()
        {
            var response = await Client.PostAsJsonAsync("/callback", new Callback
            {
                Id = Guid.NewGuid(),
                Type = CallbackType.Order,
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