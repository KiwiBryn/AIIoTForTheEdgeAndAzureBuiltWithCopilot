// please write a C# console application that uses Onnx to run a ONNX Faster-RCNN object detection model on an image loaded from disk
// Added Microsoft.ML.OnnxRuntime NuGet package
// Added System.Drawing.Common NuGet package
// Fixed up model and image paths
// resize the image such that both height and width are within the range of [800, 1333], such that both height and width are divisible by 32.
// Apply FasterRCNN mean to each channel
// Display label, confidence and bounding box
// Used Netron to fix up names
using System.Drawing;
using System.Drawing.Drawing2D;

using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;


namespace FasterRCNNObjectDetectionApplicationGitHubCopilot
{
   class Program
   {
      static void Main(string[] args)
      {
         // Path to the ONNX model and input image
         string modelPath = @"..\\..\\..\\..\\Models\\FasterRCNN-10.onnx";
         string imagePath = "sports.jpg";

         Console.WriteLine("FasterRCNNObjectDetectionApplicationGitHubCopilot");

         // Load the image
         Bitmap image = new Bitmap(imagePath);

         // Load the ONNX model
         using var session = new InferenceSession(modelPath);

         // Preprocess the image
         var inputTensor = PreprocessImage(image);

         // Run inference
         var inputs = new List<NamedOnnxValue>
         {
             NamedOnnxValue.CreateFromTensor("image", inputTensor)
         };

         using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = session.Run(inputs);

         // Postprocess the results
         PostprocessResults(results, image);

         Console.WriteLine("Press Enter to exit");
         Console.ReadLine();
      }

      static DenseTensor<float> PreprocessImage(Bitmap bitmap)
      {
         // Resize image to model's expected input size  
         Bitmap resizedImage = ResizeImageForModel(bitmap);


         // Convert image to float array and normalize
         var input = new DenseTensor<float>(new[] { 3, resizedImage.Height, resizedImage.Width });

         float[] mean = { 102.9801f, 115.9465f, 122.7717f };

         for (int y = 0; y < resizedImage.Height; y++)
         {
            for (int x = 0; x < resizedImage.Width; x++)
            {
               var pixel = resizedImage.GetPixel(x, y);

               input[0, y, x] = pixel.B - mean[0];
               input[1, y, x] = pixel.G - mean[1];
               input[2, y, x] = pixel.R - mean[2];
            }
         }

         return input;
      }

      static Bitmap ResizeImageForModel(Bitmap image)
      {
         // Define the target range and divisibility
         const int minSize = 800;
         const int maxSize = 1333;
         const int divisor = 32;

         // Get original dimensions
         int originalWidth = image.Width;
         int originalHeight = image.Height;

         // Calculate scale factor to fit within the range while maintaining aspect ratio
         float scale = Math.Min((float)maxSize / Math.Max(originalWidth, originalHeight),
                                (float)minSize / Math.Min(originalWidth, originalHeight));

         // Calculate new dimensions
         int newWidth = (int)(originalWidth * scale);
         int newHeight = (int)(originalHeight * scale);

         // Ensure dimensions are divisible by 32
         newWidth = (newWidth / divisor) * divisor;
         newHeight = (newHeight / divisor) * divisor;

         // Resize the image
         return new Bitmap(image, new Size(newWidth, newHeight));
      }

      static void PostprocessResults(IDisposableReadOnlyCollection<DisposableNamedOnnxValue> output, Bitmap image)
      {
         // Assuming the model outputs include bounding boxes, labels, and scores
         var boxes = output.First(x => x.Name == "6379").AsEnumerable<float>().ToArray();
         var labels = output.First(x => x.Name == "6381").AsEnumerable<long>().ToArray();
         var scores = output.First(x => x.Name == "6383").AsEnumerable<float>().ToArray();

         using Graphics graphics = Graphics.FromImage(image);
         graphics.SmoothingMode = SmoothingMode.AntiAlias;

         for (int i = 0; i < labels.Length; i++)
         {
            if (scores[i] < 0.5) continue; // Filter low-confidence detections

            // Extract bounding box coordinates
            float x1 = boxes[i * 4];
            float y1 = boxes[i * 4 + 1];
            float x2 = boxes[i * 4 + 2];
            float y2 = boxes[i * 4 + 3];

            // Draw bounding box
            RectangleF rect = new RectangleF(x1, y1, x2 - x1, y2 - y1);
            graphics.DrawRectangle(Pens.Red, rect.X, rect.Y, rect.Width, rect.Height);

            // Display label and confidence
            string label = $"Label: {labels[i]}, Confidence: {scores[i]:0.00}";
            graphics.DrawString(label, new Font("Arial", 12), Brushes.Yellow, new PointF(x1, y1 - 20));
         }

         // Save the image with annotations
         image.Save("output.jpg");

         Console.WriteLine("Output image saved as 'output.jpg'.");
      }
   }
}