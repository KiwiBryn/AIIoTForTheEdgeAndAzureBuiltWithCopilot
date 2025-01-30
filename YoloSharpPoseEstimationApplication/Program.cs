//please write a C# console application that uses compunet yolosharp to run an Ultralytics yolo pose estimation model on an image loaded from disk
using Compunet.YoloSharp;

using SixLabors.ImageSharp;

class Program
{
   static void Main(string[] args)
   {
      // Load the YOLO model
      var modelPath = "path_to_your_yolo_model.onnx";
      var yolo = new YoloPredictor(modelPath);

      // Load the image from disk
      //var imagePath = "path_to_your_image.jpg";
      var imagePath = "sports.jpg";
      //using var image = Image.FromFile(imagePath);
      using var image = Image.Load(imagePath);

      // Run pose estimation
      //var results = yolo.Predict(image);
      var results = yolo.Pose(image);

      // Display results
      foreach (var result in results)
      {
         //Console.WriteLine($"Object: {result.Label}, Confidence: {result.Confidence}");
         Console.WriteLine($"Object: {result.Name}, Confidence: {result.Confidence}");
         //foreach (var point in result.PosePoints)
         foreach (var point in result)
         {
            //Console.WriteLine($"Point: {point.X}, {point.Y}");
            Console.WriteLine($"Point: {point.Point.X}, {point.Point.Y}");
         }
      }
   }
}
