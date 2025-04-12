// please write a C# console application that uses Onnx to run a ONNX Faster-RCNN object detection model on an image loaded from disk
// Added Microsoft.ML.OnnxRuntime NuGet package
// Added System.Drawing.Common NuGet package
// Fixed up model and image paths
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Drawing;
using System.IO;
using System.Linq;

class Program
{
   static void Main(string[] args)
   {
      // Path to the ONNX model and input image
      string modelPath = @"..\\..\\..\\..\\Models\\FasterRCNN-10.onnx";
      string imagePath = "sports.jpg";

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
      var output = results.First().AsEnumerable<float>().ToArray();
      PostprocessResults(output);

      Console.WriteLine("Press Enter to exit");
      Console.ReadLine();
   }

   static DenseTensor<float> PreprocessImage(string imagePath)
   {
      using var bitmap = new Bitmap(imagePath);

      // Resize image to model's expected input size (e.g., 224x224)
      int targetWidth = 224;
      int targetHeight = 224;
      using var resizedBitmap = new Bitmap(bitmap, new Size(targetWidth, targetHeight));

      // Convert image to float array and normalize
      //var input = new DenseTensor<float>(new[] { 1, 3, targetHeight, targetWidth });
      var input = new DenseTensor<float>(new[] { 3, targetHeight, targetWidth });
      for (int y = 0; y < targetHeight; y++)
      {
         for (int x = 0; x < targetWidth; x++)
         {
            var pixel = resizedBitmap.GetPixel(x, y);
            //input[0, 0, y, x] = pixel.R / 255f;
            //input[0, y, x] = pixel.R / 255f;
            input[0, y, x] = pixel.B / 255f;
            //input[0, 1, y, x] = pixel.G / 255f;
            input[1, y, x] = pixel.G / 255f;
            //input[0, 2, y, x] = pixel.B / 255f;
            //input[2, y, x] = pixel.B / 255f;
            input[2, y, x] = pixel.R / 255f;
         }
      }

      return input;
   }

   static void PostprocessResults(float[] output)
   {
      // Interpret the output (e.g., bounding boxes, class probabilities)
      Console.WriteLine("Detection results:");
      foreach (var value in output)
      {
         Console.WriteLine(value);
      }
   }
}
