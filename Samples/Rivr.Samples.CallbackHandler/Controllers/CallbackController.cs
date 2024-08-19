using Microsoft.AspNetCore.Mvc;
using Rivr.Samples.CallbackHandler.Models;

namespace Rivr.Samples.CallbackHandler.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CallbackController(ILogger<CallbackController> logger) : ControllerBase
    {
        private readonly ILogger<CallbackController> _logger = logger;

        [HttpPost]
        public Task Post([FromBody] Callback callback)
        {
            _logger.LogInformation("Received callback: {Callback}", callback);
            return Task.CompletedTask;
        }
    }
}