// please write an azure function with an http trigger that uses yolosharp to estimate the pose of humans in an uploaded image.
// Yolo v8 pose estimation model and yolosharp library
// Make into azure function
// The image files are in the form of the request
// Modify the code so more than one image per request can be processed
// Initialise ILogger in the constructor
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

using Compunet.YoloSharp;

using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;

namespace YoloSharpPoseEstimationHttpTriggerFunction
{
   public class Function1
   {
      private static ILogger _log;

      public Function1(ILogger<Function1> log)
      {
         _log = log;
      }

      [Function("PoseEstimation")]
      public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
      {
         _log.LogInformation("Pose estimation function processed a request.");

         if (!req.HasFormContentType || !req.Form.Files.Any())
         {
            return new BadRequestObjectResult("Please upload image files.");
         }

         var results = new List<object>();

         foreach (var file in req.Form.Files)
         {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            using var image = Image.Load<Rgba32>(memoryStream);

            // Initialize the YOLO model
            //using var predictor = new YoloPredictor("path/to/model.onnx");
            using var predictor = new YoloPredictor("yolov8s-pose.onnx");

            // Perform pose estimation
            var result = await predictor.PoseAsync(image);

            // Format the results
            //var poses = result.Poses.Select(pose => new
            var poses = result.Select(pose => new
            {
               //Keypoints = pose.Keypoints.Select(k => new { k.X, k.Y }),
               Keypoints = pose.Select(k => new { k.Point.X, k.Point.Y, k.Confidence,k.Index }),
               Confidence = pose.Confidence,
               pose.Name,
               pose.Bounds.X, pose.Bounds.Y,pose.Bounds.Width, pose.Bounds.Height
            });

            results.Add(new
            {
               Image = file.FileName,
               Poses = poses
            });
         }

         return new OkObjectResult(new { results });
      }
   }
}

