// please write a Azure Http Trigger function that uses compunet yolosharp to run an yolov8 onnx object detection model loaded from on an image in the HTTP request body
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

      // Read the image from the request payload
      byte[] imageBytes;
      using (var memoryStream = new MemoryStream())
      {
         await req.Body.CopyToAsync(memoryStream);
         imageBytes = memoryStream.ToArray();
      }

      // Load the YOLOv8 model
      //var yolo = new Yolo("yolov8.onnx");
      var yolo = new YoloPredictor("yolov8s.onnx");

      // Perform object detection
      var items = yolo.Detect(imageBytes);

      return new OkObjectResult(items);
   }
}
/*
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
//using Microsoft.Azure.WebJobs;
//using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using YoloSharp;
using Microsoft.Azure.Functions.Worker;

public static class Function1
{
   //[FunctionName("ObjectDetectionFunction")]
   [Function("ObjectDetectionFunction")]
   public static async Task<IActionResult> Run(
       [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
       ILogger log)
   {
      log.LogInformation("C# HTTP trigger function processed a request.");

      // Read the image from the request payload
      byte[] imageBytes;
      using (var memoryStream = new MemoryStream())
      {
         await req.Body.CopyToAsync(memoryStream);
         imageBytes = memoryStream.ToArray();
      }

      // Load the YOLO model
      var yolo = new Yolo("yolov3.cfg", "yolov3.weights", "coco.names");

      // Perform object detection
      var items = yolo.Detect(imageBytes);

      // Return the detection results
      return new OkObjectResult(items);
   }
}
*/