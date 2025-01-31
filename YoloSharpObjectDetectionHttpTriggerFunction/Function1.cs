
// please write an azure function with an http trigger that uses yolosharp and an onnx file to detect objects in an image
// The image is not in the body it is in the form
// Image.Load is not used
// yolo.Detect can process an image file stream
// The YoloPredictor should be released after use
// Many image files could be uploaded in one request
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

      // Read the images from the form data
      var form = await req.ReadFormAsync();
      var files = form.Files;
      if (files.Count == 0)
      {
         return new BadRequestObjectResult("No image files uploaded.");
      }

      var results = new List<object>();

      // Load the YOLOv8 model
      using (var yolo = new YoloPredictor("yolov8s.onnx"))
      {
         foreach (var file in files)
         {
            if (file.Length > 0)
            {
               // Perform object detection
               using (var stream = file.OpenReadStream())
               {
                  var items = yolo.Detect(stream);
                  results.Add(new { FileName = file.FileName, Detections = items });
               }
            }
         }
      }

      // Return the detection results
      return new OkObjectResult(results);
   }
}
