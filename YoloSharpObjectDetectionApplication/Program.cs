// please write a C# console application that uses compunet yolosharp to run an Ultralytics yolo object detection model on an image loaded from disk
using Compunet.YoloSharp;
using SixLabors.ImageSharp;

namespace YoloSharpObjectDetectionApplication
{
   internal class Program
   {
      static void Main(string[] args)
      {
         Console.WriteLine($"{DateTime.UtcNow:yy-MM-dd HH:mm:ss} YoloSharpObjectDetectionApplication");

         // Load the YOLO model
         //var model = new YoloModel("path_to_your_model.onnx");
         var model = new YoloPredictor("..\\..\\..\\..\\Models\\yolov8s.onnx");
         //var model = new YoloPredictor("..\\..\\..\\..\\Models\\yolov10s.onnx");
         //var model = new YoloPredictor("..\\..\\..\\..\\Models\\yolov11s.onnx");

         // Load the image from disk
         //var image = Image.FromFile("path_to_your_image.jpg");
         var image = Image.Load("sports.jpg");

         // Run object detection
         //var results = model.Predict(image);
         var results = model.Detect(image);

         // Display the results
         foreach (var result in results)
         {
            //Console.WriteLine($"Object: {result.Label}, Confidence: {result.Confidence}, Bounding Box: {result.BoundingBox}");
            Console.WriteLine($"Object: {result.Name}, Confidence: {result.Confidence}, Bounding Box: {result.Bounds}");
         }
      }
   }
}
