// please write a C# console application that uses Onnx to run a ONNX Faster-RCNN object detection model on an image loaded from disk
// Added Microsoft.ML.OnnxRuntime NuGet package
// Added System.Drawing.Common NuGet package
// Fixed up model and image paths
// resize the image such that both height and width are within the range of [800, 1333], such that both height and width are divisible by 32.
// Apply FasterRCNN mean to each channel
// Display label, confidence and bounding box
// Used Netron to fix up names
using System.Drawing;

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

         // Load the ONNX model
         using var session = new InferenceSession(modelPath);

         // Preprocess the image
         var inputTensor = PreprocessImage(imagePath);

         // Run inference
         var inputs = new List<NamedOnnxValue>
           {
               NamedOnnxValue.CreateFromTensor("image", inputTensor)
           };

         using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = session.Run(inputs);

         // Postprocess the results
         PostprocessResults(results);

         Console.WriteLine("Press Enter to exit");
         Console.ReadLine();
      }

      static DenseTensor<float> PreprocessImage(string imagePath)
      {
         using var bitmap = new Bitmap(imagePath);

         const int minSize = 800;
         const int maxSize = 1333;

         // Calculate new dimensions within the range [800, 1333] and divisible by 32
         int originalWidth = bitmap.Width;
         int originalHeight = bitmap.Height;

         float scale = Math.Min(1333.0f / Math.Max(originalWidth, originalHeight), 800.0f / Math.Min(originalWidth, originalHeight));

         int targetWidth = (int)(originalWidth * scale);
         int targetHeight = (int)(originalHeight * scale);

         targetWidth = (targetWidth / 32) * 32;
         targetHeight = (targetHeight / 32) * 32;

         using var resizedBitmap = new Bitmap(bitmap, new Size(targetWidth, targetHeight));

         // Convert image to float array and normalize
         var input = new DenseTensor<float>(new[] { 3, targetHeight, targetWidth });

         float[] mean = { 102.9801f, 115.9465f, 122.7717f };

         for (int y = 0; y < targetHeight; y++)
         {
            for (int x = 0; x < targetWidth; x++)
            {
               var pixel = resizedBitmap.GetPixel(x, y);

               input[0, y, x] = pixel.B - mean[0];
               input[1, y, x] = pixel.G - mean[1];
               input[2, y, x] = pixel.R - mean[2];
            }
         }

         return input;
      }

      static void PostprocessResults(IDisposableReadOnlyCollection<DisposableNamedOnnxValue> output)
      {
         // Assuming the model outputs include bounding boxes, labels, and scores
         var boxes = output.First(x => x.Name == "6379").AsEnumerable<float>().ToArray();
         var labels = output.First(x => x.Name == "6381").AsEnumerable<long>().ToArray();
         var scores = output.First(x => x.Name == "6383").AsEnumerable<float>().ToArray();

         Console.WriteLine("Detection results:");
         for (int i = 0; i < labels.Length; i++)
         {
            if (scores[i] > 0.75) // Display results with confidence > 0.75
            {
               Console.WriteLine($"Label: {labels[i]}, Confidence: {scores[i]:0.00}, " +
                                 $"Bounding Box: [X1: {boxes[i * 4]}, Y1: {boxes[i * 4 + 1]}, " +
                                 $"X2: {boxes[i * 4 + 2]}, Y2: {boxes[i * 4 + 3]}]");
            }
         }
      }
   }
}