using System.Net.Http.Json;
using System.Text.Json;
using Rivr.Models.Orders;
using Rivr.Samples.CallbackHandler.Models;

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
                Type = nameof(Order),
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