using SkiaSharp;

using YoloDotNet;
using YoloDotNet.Models;
using YoloDotNet.Enums;

namespace YoloDotNetObjectDetectionApplicationCopilot
{
   internal class Program
   {
      static void Main(string[] args)
      {
         string modelPath = "yolov8s.onnx"; // Replace with your actual model path
         string imagePath = "sports.jpg"; // Replace with your actual image path

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
               ModelType = ModelType.ObjectDetection,
               OnnxModel = modelPath,
               Cuda = false
            };

            using var yolo = new Yolo(yoloOptions);

            // Convert SKBitmap to a format YOLO can process
            using var skImage = SKImage.FromEncodedData(imagePath);

            var results = yolo.RunObjectDetection(skImage);

            foreach (var result in results)
            {
               Console.WriteLine($"Detected: {result.Label} - Confidence: {result.Confidence:F2}");
               Console.WriteLine($"Bounding Box: {result.BoundingBox}");
            }
         }
         catch (Exception ex)
         {
            Console.WriteLine($"Error: {ex.Message}");
         }

         Console.WriteLine("Press Enter to exit the application");
         Console.ReadLine();
      }
   }
}