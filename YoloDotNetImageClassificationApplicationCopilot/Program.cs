using System;
using System.Drawing;
using YoloDotNet; // Placeholder (Ensure correct namespace)
using YoloDotNet.Models;

class Program
   {
      static void Main(string[] args)
      {
         string modelPath = "path/to/yolo_model.onnx";  // Update with actual model path
         string imagePath = "path/to/image.jpg";        // Update with actual image path

         // Load YOLO model
         var yolo = new YoloModel(modelPath);

         // Load image
         using Bitmap image = new Bitmap(imagePath);

         // Run classification
         var results = yolo.Predict(image);

         // Display results
         foreach (var result in results)
         {
            Console.WriteLine($"Detected: {result.Label} with confidence {result.Confidence:P}");
         }
      }
   }
}

