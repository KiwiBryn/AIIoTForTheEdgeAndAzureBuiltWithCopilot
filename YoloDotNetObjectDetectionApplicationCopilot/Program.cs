//using System.Drawing;
using SkiaSharp;

using YoloDotNet;
using YoloDotNet.Models;

namespace YoloDotNetObjectDetectionApplicationCopilot
{
   internal class Program
   {
      static void Main(string[] args)
      {
         string modelPath = "yolov8.onnx"; // Replace with your actual model path
         string imagePath = "image.jpg"; // Replace with your actual image path

         if (!File.Exists(modelPath))
         {
            Console.WriteLine("Error: Model file not found!");
            return;
         }

         if (!File.Exists(imagePath))
         {
            Console.WriteLine("Error: Image file not found!");
            return;
         }

         try
         {
            var yoloOptions = new YoloOptions
            {
               ConfidenceThreshold = 0.5, // Confidence threshold (adjust as needed)
               IoUThreshold = 0.4        // Intersection over Union threshold
            };

            // Load the YOLO model
            //using var yolo = new Yolo(modelPath);
            //using var yolo = new Yolo(yoloOptions);
            //using var yolo = new Yolo(modelPath, yoloOptions);
            using var yolo = new Yolo(yoloOptions);

            // Load image using SkiaSharp
            using var skBitmap = SKBitmap.Decode(imagePath);

            // Convert SKBitmap to a format YOLO can process
            using var skImage = SKImage.FromBitmap(skBitmap);
            using var skData = skImage.Encode(SKEncodedImageFormat.Jpeg, 100);
            using var memoryStream = new MemoryStream(skData.ToArray());
            //var results = yolo.Predict(memoryStream);
            var results = yolo.RunObbDetection(skImage);

            // Display detected objects
            foreach (var result in results)
            {
               Console.WriteLine($"Detected: {result.Label} - Confidence: {result.Confidence}");
               Console.WriteLine($"Bounding Box: {result.BoundingBox}");
            }
         }
         catch (Exception ex)
         {
            Console.WriteLine($"Error: {ex.Message}");
         }
      }
   }
}