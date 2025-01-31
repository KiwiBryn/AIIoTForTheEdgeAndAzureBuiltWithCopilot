// please write an httpTrigger azure function that uses YoloSharp and a Yolo image classification onnx model
// Image classification not object detection
// The image is in the form data
// The multipart/form-data check can be removed
// The YoloPredictor should be released after use
// Many image files could be uploaded in one request
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

using Compunet.YoloSharp;
using Compunet.YoloSharp.Data;

namespace YoloSharpImageClassificationHttpTriggerFunction
{
   public class Function1
   {
      private readonly ILogger<Function1> _logger;

      public Function1(ILogger<Function1> logger)
      {
         _logger = logger;
      }

      [Function("ImageClassificationFunction")]
      public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
      {
         _logger.LogInformation("C# HTTP trigger function processed a request.");

         var form = await req.ReadFormAsync();
         var files = form.Files;

         if (files.Count == 0)
         {
            return new BadRequestObjectResult("No images uploaded.");
         }

         var results = new List<object>();

         foreach (var file in files)
         {
            if (file.Length > 0)
            {
               using (var memoryStream = new MemoryStream())
               {
                  await file.CopyToAsync(memoryStream);
                  var imageBytes = memoryStream.ToArray();

                  using (var yoloModel = new YoloPredictor("yolov8s-cls.onnx"))
                  {
                     var classifications = yoloModel.Classify(imageBytes);

                     results.Add(new { file.FileName, classifications });
                  }
               }
            }
         }

         return new OkObjectResult(results);
      }
   }
}
