// please write a C# console application that uses compunet yolosharp to run an Ultralytics yolo image classification model on an image loaded from disk
//   Don't understand why copilot auto completed this as well?
// the path to the image file should be provided as the first command line argument to the application
using Compunet.YoloSharp;

using SixLabors.ImageSharp;

namespace YoloSharpImageClassificationApplication
{
   internal class Program
   {
      static void Main(string[] args)
      {
         Console.WriteLine($"{DateTime.UtcNow:yy-MM-dd HH:mm:ss} YoloSharpImageClassificationApplication");

         // Check if an image path is provided
         if (args.Length == 0)
         {
            Console.WriteLine("Please provide the path to an image file.");
            return;
         }

         string imagePath = args[0];

         // Load the YOLO model
         //var model = new YoloModel("path_to_your_model.onnx");
         //var model = new YoloPredictor("..\\..\\..\\..\\models\\yolov8s-cls.onnx");
         //var model = new YoloPredictor("..\\..\\..\\..\\models\\yolov10s-cls.onnx"); // Wasn't able to find yolov10s-cls.onnx
         var model = new YoloPredictor("..\\..\\..\\..\\models\\yolov11s-cls.onnx");

         // Load the image from disk
         //using (var image = Image.FromFile(imagePath))
         using (var image = Image.Load(imagePath))
         {
            // Run the image classification
            //var results = model.Predict(image);
            var results = model.Classify(image);

            // Display the results
            foreach (var result in results)
            {
               //Console.WriteLine($"Label: {result.Label}, Confidence: {result.Confidence}");
               Console.WriteLine($"Label: {result.Name}, Confidence: {result.Confidence}");
            }
         }
      }
   }
}
