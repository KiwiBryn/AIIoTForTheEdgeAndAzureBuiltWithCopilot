
//please write an azure function with an http trigger that uses yolosharp to detect objects in an image
// The image is not in the body it is in the form
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Compunet.YoloSharp;
using Microsoft.Azure.Functions.Worker;
using SixLabors.ImageSharp;

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

      // Read the image from the form data
      var form = await req.ReadFormAsync();
      var file = form.Files["image"];
      if (file == null || file.Length == 0)
      {
         return new BadRequestObjectResult("Image file is missing or empty.");
      }

      byte[] imageBytes;
      using (var memoryStream = new MemoryStream())
      {
         await file.CopyToAsync(memoryStream);
         imageBytes = memoryStream.ToArray();
      }

      var image = Image.Load(imageBytes);

      // Load the YOLOv8 model
      var yolo = new YoloPredictor("yolov8s.onnx");

      // Perform object detection
      var items = yolo.Detect(imageBytes);

      // Return the detection results
      return new OkObjectResult(items);
   }
}
