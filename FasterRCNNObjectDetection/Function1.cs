using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FasterRCNNObjectDetection;

public class Function1
{
   private readonly ILogger<Function1> _logger;

   public Function1(ILogger<Function1> logger)
   {
      _logger = logger;
   }

   [Function("ObjectDetectionFunction")]
   public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
   {
      _logger.LogInformation("C# HTTP trigger function processed a request.");

      return new OkObjectResult("Welcome to Azure Functions!");
   }
}