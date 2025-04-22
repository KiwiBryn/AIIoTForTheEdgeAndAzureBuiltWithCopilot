using System;
using System.Drawing;
using System.Linq;
using System.IO;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

class Program
{
   static void Main()
   {
      string modelPath = "resnet50-v2-7.onnx"; // Updated model path
      string imagePath = "pizza.jpg"; // Updated image path

      using var session = new InferenceSession(modelPath);
      var inputTensor = LoadAndPreprocessImage(imagePath);

      var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("data", inputTensor) // Using "data" as the input tensor name
        };

      using var results = session.Run(inputs);
      var output = results.First().AsTensor<float>().ToArray();

      // Calculate softmax
      var probabilities = Softmax(output);

      // Get the class index with the highest probability
      int predictedClass = Array.IndexOf(probabilities, probabilities.Max());
      Console.WriteLine($"Predicted class index: {predictedClass}");
      Console.WriteLine($"Probabilities: {string.Join(", ", probabilities.Select(p => p.ToString("F4")))}");
   }

   static DenseTensor<float> LoadAndPreprocessImage(string imagePath)
   {
      using Bitmap bitmap = new Bitmap(imagePath);
      int width = 224, height = 224; // ResNet50 expects 224x224 input
      using Bitmap resized = new Bitmap(bitmap, new Size(width, height));

      var tensor = new DenseTensor<float>(new[] { 1, 3, width, height });

      // ImageNet mean & standard deviation values
      float[] mean = { 0.485f, 0.456f, 0.406f };
      float[] stdev = { 0.229f, 0.224f, 0.225f };

      for (int y = 0; y < height; y++)
      {
         for (int x = 0; x < width; x++)
         {
            Color pixel = resized.GetPixel(x, y);

            // Normalize using mean and standard deviation
            tensor[0, 0, y, x] = (pixel.R / 255f - mean[0]) / stdev[0]; // Red channel
            tensor[0, 1, y, x] = (pixel.G / 255f - mean[1]) / stdev[1]; // Green channel
            tensor[0, 2, y, x] = (pixel.B / 255f - mean[2]) / stdev[2]; // Blue channel
         }
      }

      return tensor;
   }

   static float[] Softmax(float[] logits)
   {
      // Compute softmax
      var expScores = logits.Select(Math.Exp).ToArray();
      double sumExpScores = expScores.Sum();
      return expScores.Select(score => (float)(score / sumExpScores)).ToArray();
   }
}