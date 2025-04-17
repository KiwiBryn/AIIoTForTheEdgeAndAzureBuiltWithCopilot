using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
//using Microsoft.Azure.WebJobs.Extensions.Http; BHL
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;


// Assuming YoloSharp exposes a detector object and model inference methods.
//using YoloSharp; BHL
using Compunet.YoloSharp;
using Compunet.YoloSharp.Data;
using SixLabors.ImageSharp;

namespace ObjectDetectionFunctionApp
{
   //public static class ObjectDetectionFunction
   public class ObjectDetectionFunction
   {
      private readonly ILogger log;

      public ObjectDetectionFunction(ILogger<ObjectDetectionFunction> log)
      {
         this.log = log;
      }

      //[FunctionName("DetectObjects")] BHL
      //public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "detect")] HttpRequest req,ILogger log) BHL
      [Function("ObjectDetectionFunction")]
      public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
      {
         log.LogInformation("DetectObjects function received a request for object detection.");

         // Ensure the request content type is an image.
         if (!req.ContentType.Contains("image"))
         {
            return new BadRequestObjectResult("Please provide an image file in the request body.");
         }

         // Read the image from the request body.
         byte[] imageBytes;
         using (var ms = new MemoryStream())
         {
            await req.Body.CopyToAsync(ms);
            imageBytes = ms.ToArray();
         }

         if (imageBytes.Length == 0)
         {
            return new BadRequestObjectResult("The provided image is empty.");
         }

         try
         {
            // Build the path to the ONNX model.
            // In this example, the ONNX file is stored in a "models" folder within the function app.
            string modelDirectory = Path.Combine(Environment.CurrentDirectory, "models");
            //string onnxModelPath = Path.Combine(modelDirectory, "yolo_model.onnx"); BHL 
            string onnxModelPath = Path.Combine(modelDirectory, "yolov8s.onnx");

            if (!File.Exists(onnxModelPath))
            {
               return new BadRequestObjectResult("ONNX model file not found at: " + onnxModelPath);
            }

            // Initialize the YoloSharp detector.
            // The detector’s constructor loads and initializes the ONNX model.
            //var detector = new YoloDetector(onnxModelPath); BHL
            var detector = new YoloPredictor(onnxModelPath);

            // Run the detection; this method is assumed to accept a byte array or stream.
            // Adjust the method signature if your version of YoloSharp uses a different pattern.
            //List<DetectionResult> detectionResults = detector.Detect(imageBytes); BHL
            //List<DetectionResult> detectionResults = detector.Detect(imageBytes); BHL
            YoloResult<Detection> detectionResults = detector.Detect(imageBytes);

            // Return the detection results as JSON.
            return new OkObjectResult(detectionResults);
         }
         catch (Exception ex)
         {
            log.LogError(ex, "Error during object detection.");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
         }
      }
   }
}

/*
   // Below are example classes for formatting detection results.
   // Your YoloSharp library may provide similar types out-of-the-box.
   public class DetectionResult
   {
      public string Label { get; set; }
      public float Confidence { get; set; }
      public BoundingBox BBox { get; set; }
   }

   public class BoundingBox
   {
      // Coordinates and dimensions relative to the image.
      public float X { get; set; }
      public float Y { get; set; }
      public float Width { get; set; }
      public float Height { get; set; }
   }
*/

/*
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace YoloSharpObjectDetectionHttpTriggerFunctionCopilot
{
    public class Function1
    {
        private readonly ILogger<Function1> _logger;

        public Function1(ILogger<Function1> logger)
        {
            _logger = logger;
        }

        [Function("Function1")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            return new OkObjectResult("Welcome to Azure Functions!");
        }
    }
}
*/