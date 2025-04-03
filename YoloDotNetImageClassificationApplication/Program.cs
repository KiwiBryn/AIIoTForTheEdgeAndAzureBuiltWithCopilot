// please write a C# console application that uses YoloDotNet to run an Ultralytics yolo image classification model on an image loaded from disk
// From https://github.com/AlturosDestinations/Alturos.Yolo/blob/master/src/Alturos.Yolo/YoloWrapper.cs
// Modify code to use SkiaSharp
// 
using SkiaSharp;
using YoloDotNet;


namespace ONNXYoloDotNetImageClassificationApplication
{
   internal class Program
   {
      static void Main(string[] args)
      {
         if (args.Length == 0)
         {
            Console.WriteLine("Please provide the path to the image file.");
            return;
         }

         string imagePath = args[0];

         if (!File.Exists(imagePath))
         {
            Console.WriteLine("The specified image file does not exist.");
            return;
         }

         // Load the YOLO model
         //var yolo = new YoloWrapper("yolov3-ultralytics.onnx");
         var yolo = new Yolo(new YoloDotNet.Models.YoloOptions()
         {
            ModelType = YoloDotNet.Enums.ModelType.Classification,
            OnnxModel = "..\\..\\..\\..\\Models\\yolov8s-cls.onnx", // Offered up object detection model
            Cuda = false, // Set to true if you have a compatible GPU and want to use CUDA
            GpuId = 0, // Set to the GPU ID you want to use (if applicable)
         });

         // Load the image using SkiaSharp
         using (var image = SKImage.FromEncodedData(imagePath))
         {
            // Perform object detection
            var items = yolo.RunClassification(image);

            // Display the results
            foreach (var item in items)
            {
               Console.WriteLine($"Object: {item.Label}, Confidence: {item.Confidence}");
            }
         }

         Console.WriteLine("Press Enter to exit the application");
         Console.ReadLine();
      }
   }
}

