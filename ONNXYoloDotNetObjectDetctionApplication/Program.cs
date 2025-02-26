// Use YoloDotNet to run an onnx Object Detection model on the image loaded from disk
// Modify code to use SkiaSharp
// Modify code to use YoloConfiguration whne Yolo is created
using SkiaSharp;
using System;
using System.Drawing;
using System.IO;
using YoloDotNet;
using YoloDotNet.Models;
//using YoloDotNet.YoloOnnx;

class Program
{
   static void Main(string[] args)
   {
      // Load the ONNX model
      var modelPath = "path/to/your/model.onnx";
      //var yoloModel = new YoloDotNet(modelPath);
      var yoloModel = new YoloDotNet.Yolo(modelPath);

      // Load the image from disk
      var imagePath = "path/to/your/image.jpg";
      using var inputStream = File.OpenRead(imagePath);
      using var skBitmap = SKBitmap.Decode(inputStream);

      // Convert SKBitmap to byte array
      using var image = new SKImage(skBitmap);
      using var data = image.Encode(SKEncodedImageFormat.Jpeg, 100);
      var imageBytes = data.ToArray();

      // Run object detection
      var results = yoloModel.Detect(imageBytes);

      // Process and display the results
      foreach (var result in results)
      {
         Console.WriteLine($"Object: {result.Label}, Confidence: {result.Confidence}, BoundingBox: {result.BoundingBox}");
      }

      // Optionally, draw bounding boxes on the image and save it
      using var canvas = new SKCanvas(skBitmap);
      var paint = new SKPaint
      {
         Color = SKColors.Red,
         Style = SKPaintStyle.Stroke,
         StrokeWidth = 2
      };

      foreach (var result in results)
      {
         var rect = new SKRect(result.BoundingBox.X, result.BoundingBox.Y, result.BoundingBox.X + result.BoundingBox.Width, result.BoundingBox.Y + result.BoundingBox.Height);
         canvas.DrawRect(rect, paint);
      }

      using var outputStream = File.OpenWrite("output.jpg");
      skBitmap.Encode(outputStream, SKEncodedImageFormat.Jpeg, 100);
   }
}

/*
// please write a C# console application that uses YoloDotNet by Niklas Swärd to run an Ultralytics yolo object detection model on an image loaded from disk
namespace ONNXYoloDotNetObjectDetctionApplication
{
   internal class Program
   {
      static void Main(string[] args)
      {
         if (args.Length < 3)
         {
            Console.WriteLine("Usage: <executable> <model config path> <weights path> <image path>");
            return;
         }

         string modelConfigPath = args[0];
         string weightsPath = args[1];
         string imagePath = args[2];

         if (!File.Exists(modelConfigPath) || !File.Exists(weightsPath) || !File.Exists(imagePath))
         {
            Console.WriteLine("One or more files do not exist.");
            return;
         }

         using (var yoloWrapper = new YoloWrapper(modelConfigPath, weightsPath))
         {
            using (var image = Image.FromFile(imagePath))
            {
               var memoryStream = new MemoryStream();
               image.Save(memoryStream, image.RawFormat);
               var imageBytes = memoryStream.ToArray();

               var items = yoloWrapper.Detect(imageBytes);

               foreach (var item in items)
               {
                  Console.WriteLine($"Object: {item.Type}, Confidence: {item.Confidence}, X: {item.X}, Y: {item.Y}, Width: {item.Width}, Height: {item.Height}");
               }
            }
         }
      }
   }
}
*/