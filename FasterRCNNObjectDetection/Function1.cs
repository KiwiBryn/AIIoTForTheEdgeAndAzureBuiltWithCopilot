using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.ML.OnnxRuntime;

namespace FasterRCNNObjectDetection;

public class Function1
{
   private readonly ILogger<Function1> _logger;
   private readonly List<string> _labels;
   private readonly InferenceSession _session;

   public Function1(ILogger<Function1> logger)
   {
      _logger = logger;
      _labels = File.ReadAllLines("labels.txt").ToList();
      _session = new InferenceSession("FasterRCNN-10.onnx");
   }

   [Function("ObjectDetectionFunction")]
   public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
   {
      _logger.LogInformation("C# HTTP trigger function processed a request.");

      return new OkObjectResult("Welcome to Azure Functions!");
   }
}