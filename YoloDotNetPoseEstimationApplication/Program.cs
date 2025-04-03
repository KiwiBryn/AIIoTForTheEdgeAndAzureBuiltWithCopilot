// please write a C# console application that uses YoloDotNet V2 by Niklas Swärd to run an Ultralytics yolo pose estimation model on an image loaded from disk
// Use YoloOptions to configure the model
// Use SKImage to load the image from disk to use for pose estimation  
// Completely hopeless
namespace ONNXYoloDotNetPoseEstimationApplication
{
   using SkiaSharp;
   using System;
   //using System.Drawing;
   using YoloDotNet;
   using YoloDotNet.Models;

   class Program
   {
      // Load the ONNX model
      static void Main(string[] args)
      {
         // var modelPath = "path/to/your/model.onnx";
         string modelPath = "..\\..\\..\\..\\Models\\yolov8s-pose.onnx";

         // Path to the image
         //string imagePath = "path/to/your/image.jpg";
         string imagePath = "sports.jpg";
         using SKImage image = SKImage.FromEncodedData(imagePath);

         // Configure YOLO model options
         var options = new YoloOptions
         {
            OnnxModel = modelPath,
             ModelType = YoloDotNet.Enums.ModelType.PoseEstimation, // Set to PoseEstimation
            Cuda = false, // Set to true if you want to use GPU acceleration  
            /*
            ConfidenceThreshold = 0.5f, // Adjust as needed
            IoUThreshold = 0.45f, // Intersection over Union threshold
            UseGPU = true // Enable GPU acceleration if available
            */
         };

         // Load the YOLO model with options
         //using var yoloModel = new YoloModel(options);
         using var yoloModel = new Yolo(options);

         // Run pose estimation
         //var results = yoloModel.Predict(image);
         var results = yoloModel.RunPoseEstimation(image);

         // Display the results
         foreach (var result in results)
         {
            Console.WriteLine($"Object: {result.Label}, Confidence: {result.Confidence}");
            foreach (var keypoint in result.KeyPoints)
            {
               Console.WriteLine($"Keypoint: {keypoint.X}, {keypoint.Y}");
            }
         }

         Console.WriteLine("Press Enter to exit");
         Console.ReadLine();
      }
   }
}


/*
 namespace ONNXYoloDotNetPoseEstimationApplication
{
   class Program
   {
      static void Main(string[] args)
      {
         // Path to the image
         string imagePath = "path/to/your/image.jpg";
         Bitmap image = new Bitmap(imagePath);

         // Load the YOLO model
         string modelPath = "path/to/yolo/model.onnx";
         using var yoloModel = new YoloModel(modelPath);

         // Run pose estimation
         var results = yoloModel.Predict(image);

         // Display the results
         foreach (var result in results)
         {
            Console.WriteLine($"Object: {result.Label}, Confidence: {result.Confidence}");
            foreach (var keypoint in result.Keypoints)
            {
               Console.WriteLine($"Keypoint: {keypoint.X}, {keypoint.Y}");
            }
         }

         Console.WriteLine("Pose estimation complete.");
      }
   }
}
*/
