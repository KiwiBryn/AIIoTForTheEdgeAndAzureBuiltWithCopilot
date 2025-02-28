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
   public class Function1
   {
      private readonly ILogger<Function1> _logger;
      //private readonly YoloWrapper _yolo; // This is from Alturos.Yolo
      private readonly Yolo _yolo; // This is from YoloDotNet

      public Function1(ILogger<Function1> logger)
      {
         _logger = logger;
         //_yolo = new YoloWrapper("yolov3.cfg", "yolov3.weights", "coco.names"); // This is from Alturos.Yolo
         _yolo = new Yolo(new YoloDotNet.Models.YoloOptions()
         {
            Cuda = false,
            OnnxModel = "yolov8s.onnx",
            ModelType = ModelType.ObjectDetection,
         });
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
            //using (var image = Image.FromStream(stream)) // From System.Drawing
            //{
            //   var items = _yolo.Detect(image);
            //   return new OkObjectResult(items);
            //}
            stream.Seek(0, SeekOrigin.Begin);
            //using (var skiaStream = new SKManagedStream(stream))
            //using (var image = SKBitmap.Decode(skiaStream))
            using( SKImage image = SKImage.FromEncodedData(stream))
            {
               var items = _yolo.RunObjectDetection(image);

               return new OkObjectResult(items);
            }
         }
      }
   }
}
