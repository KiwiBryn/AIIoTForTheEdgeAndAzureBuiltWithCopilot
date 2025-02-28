// Modify the code so it uses YoloDotNet to detect objects an image in the POST form data
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace YoloDotnetObjectDetectionHttpTriggerFunction
{
   public class Function1
   {
      private readonly ILogger<Function1> _logger;
      private readonly YoloWrapper _yolo; // This is from Alturos.Yolo

      public Function1(ILogger<Function1> logger)
      {
         _logger = logger;
         _yolo = new YoloWrapper("yolov3.cfg", "yolov3.weights", "coco.names"); // This is from Alturos.Yolo
      }

      [Function("ObjectDetectionFunction")]
      public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
      {
         _logger.LogInformation("C# HTTP trigger function processed a request.");

         if (!req.HasFormContentType || !req.Form.Files.Any())
         {
            return new BadRequestObjectResult("Please upload an image file.");
         }

         var file = req.Form.Files[0];
         using (var stream = new MemoryStream())
         {
            await file.CopyToAsync(stream);
            using (var image = Image.FromStream(stream)) // From System.Drawing
            {
               var items = _yolo.Detect(image);
               return new OkObjectResult(items);
            }
         }
      }
   }
}
