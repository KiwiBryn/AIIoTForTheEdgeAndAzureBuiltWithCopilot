// please write an azure function with an http trigger that uses yolosharp and an onnx file to detect objects in an image
// The image is not in the body it is in the form
// Image.Load is not used
// yolo.Detect can process an image file stream
// The YoloPredictor should be released after use
// Many image files could be uploaded in one request
// Only one image per request
// Add multipart/form-data content type check 
// Add check that there are headers -- Didn't get this right
// Add check that req.Headers is not null

// Add check that request has ContentType and it is multipart/form-data
//using System.IO;
//using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Compunet.YoloSharp;

public class Function1
{
   private readonly ILogger<Function1> _logger;

   public Function1(ILogger<Function1> logger)
   {
      _logger = logger;
   }

   [Function("ObjectDetectionFunction")]
   public async Task<IActionResult> Run(
       [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
   {
      _logger.LogInformation("C# HTTP trigger function processed a request.");

      //  if (req.Headers == null || !req.Headers.ContainsKey("Content-Type") || !req.Headers["Content-Type"].ToString().Contains("multipart/form-data"))
      //  {
      //     return new BadRequestObjectResult("The request must have a Content-Type header with multipart/form-data.");
      //  }

      // Intellisense fix
      if (req.Headers == null || !req.Headers.TryGetValue("Content-Type", out Microsoft.Extensions.Primitives.StringValues value) || !value.ToString().Contains("multipart/form-data"))
      {
         return new BadRequestObjectResult("The request must have a Content-Type header with multipart/form-data.");
      }

      // Read the images from the form data
      var form = await req.ReadFormAsync();
      var files = form.Files;
      if (files.Count == 0)
      {
         return new BadRequestObjectResult("No image files uploaded.");
      }

      if (files.Count > 1)
      {
         return new BadRequestObjectResult("Only one image file is allowed per request.");
      }

      var file = files[0];
      if (file.Length == 0)
      {
         return new BadRequestObjectResult("The uploaded image file is empty.");
      }

      // Load the YOLOv8 model
      using (var yolo = new YoloPredictor("yolov8s.onnx"))
      {
         // Perform object detection
         using (var stream = file.OpenReadStream())
         {
            var items = yolo.Detect(stream);
            var result = new { FileName = file.FileName, Detections = items };

            // Return the detection results
            return new OkObjectResult(result);
         }
      }
   }
}
