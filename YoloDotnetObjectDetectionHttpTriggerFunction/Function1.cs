// Modify the code so it uses YoloDotNet to detect objects an image in the POST form data
// Modify the code to use YoloDotNet v2.2 from https://github.com/NickSwardh/YoloDotNet
// Modify the code to use YoloDotNet
// Modify the code to use SkiaSharp to load the image
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

using SkiaSharp;

using YoloDotNet;
using YoloDotNet.Enums;


namespace YoloDotnetObjectDetectionHttpTriggerFunction
{
   public class Function1(ILogger<Function1> logger)
   {
      private readonly ILogger<Function1> _logger = logger;

      [Function("ObjectDetectionFunction")]
      public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
      {
         _logger.LogInformation("C# HTTP trigger function processed a request.");

         if (!req.HasFormContentType || req.Form.Files.Count != 1)
         {
            return new BadRequestObjectResult("Please upload an image file.");
         }

         var file = req.Form.Files[0];
         using (var stream = file.OpenReadStream())
         {
            using (SKImage image = SKImage.FromEncodedData(stream))
            {
               using (Yolo yolo = new Yolo(new YoloDotNet.Models.YoloOptions()
               {
                  Cuda = false,
                  OnnxModel = "yolov8s.onnx",
                  ModelType = ModelType.ObjectDetection,
               }))
               {
                  var detections = yolo.RunObjectDetection(image);

                  var results = new { file.FileName, detections = detections.Select(detection => new { detection.Confidence, detection.Label, detection.BoundingBox.Left, detection.BoundingBox.Bottom, detection.BoundingBox.Right, detection.BoundingBox.Top }) };

                  return new OkObjectResult(results);
               }
            }
         }
      }
   }
}
