using System;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.IO;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

class Program
{
   static void Main()
   {
      string modelPath = "resnet50-v2-7.onnx"; // Path to your ONNX model
      string imagePath = "pizza.jpg"; // Path to the input image

      using var session = new InferenceSession(modelPath);
      var inputTensor = LoadAndPreprocessImage(imagePath);

      var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("data", inputTensor) // Changed "input" to "data"
        };

      using var results = session.Run(inputs);
      var output = results.First().AsTensor<float>().ToArray();

      Console.WriteLine("Predicted class index: " + Array.IndexOf(output, output.Max()));
   }

   static DenseTensor<float> LoadAndPreprocessImage(string imagePath)
   {
      using Bitmap bitmap = new Bitmap(imagePath);
      int width = 224, height = 224; // ResNet50 expects 224x224 input
      using Bitmap resized = new Bitmap(bitmap, new Size(width, height));

      var tensor = new DenseTensor<float>(new[] { 1, 3, width, height });
      for (int y = 0; y < height; y++)
      {
         for (int x = 0; x < width; x++)
         {
            Color pixel = resized.GetPixel(x, y);
            tensor[0, 0, y, x] = pixel.R / 255f; // Normalize
            tensor[0, 1, y, x] = pixel.G / 255f;
            tensor[0, 2, y, x] = pixel.B / 255f;
         }
      }

      return tensor;
   }
}