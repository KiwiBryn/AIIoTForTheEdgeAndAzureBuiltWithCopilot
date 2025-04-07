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
            // Load the YOLO model
            using var yolo = new Yolo(modelPath);

            // Load image from disk
            using var image = new Bitmap(imagePath);

            // Run object detection
            var results = yolo.Predict(image);

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