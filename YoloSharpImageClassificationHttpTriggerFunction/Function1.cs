// please write an  httpTrigger azure function that uses YoloSharp and a Yolo image classification onnx model
// Image classification not object detection
// The image is in the form data
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

using Compunet.YoloSharp;

namespace YoloSharpImageClassificationHttpTriggerFunction
{
   public class Function1
   {
      private readonly ILogger<Function1> _logger;
      //private readonly YoloModel _yoloModel;
      private readonly YoloPredictor _yoloModel;

      public Function1(ILogger<Function1> logger)
      {
         _logger = logger;
         _yoloModel = new YoloPredictor("yolov8s-cls.onnx");
      }

      [Function("ImageClassificationFunction")]
      public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
      {
         _logger.LogInformation("C# HTTP trigger function processed a request.");

         if (!req.ContentType.StartsWith("multipart/form-data"))
         {
            return new BadRequestObjectResult("Invalid content type. Please upload an image in form data.");
         }

         var form = await req.ReadFormAsync();
         var file = form.Files["image"];

         if (file == null || file.Length == 0)
         {
            return new BadRequestObjectResult("No image uploaded.");
         }

         using (var memoryStream = new MemoryStream())
         {
            await file.CopyToAsync(memoryStream);
            var imageBytes = memoryStream.ToArray();

            var results = _yoloModel.Classify(imageBytes);

            return new OkObjectResult(results);
         }
      }
   }
}
