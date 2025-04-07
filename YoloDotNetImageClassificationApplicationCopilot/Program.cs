// https://github.com/ksasaosrc/YoloSharpTest
using SkiaSharp;

using YoloDotNet; 
using YoloDotNet.Enums;
using YoloDotNet.Models;

namespace YoloDotNetImageClassificationApplicationCopilot
{
   class Program
   {
      static void Main(string[] args)
      {
         string modelPath = "yolov8s-cls.onnx";  // Update with actual model path
         string imagePath = "pizza.jpg";        // Update with actual image path

         var yolo = new Yolo(new YoloOptions()
         {
            ModelType = ModelType.Classification,
            OnnxModel = modelPath,
            Cuda = false
         });

         // Load image
         using SKImage image = SKImage.FromEncodedData(imagePath);

         // Run classification
         var results = yolo.RunClassification(image);

         // Display results
         foreach (var result in results)
         {
            Console.WriteLine($"Detected: {result.Label} with confidence {result.Confidence:P}");
         }

         Console.WriteLine("Press Enter to exit the application");
         Console.ReadLine();
      }
   }
}