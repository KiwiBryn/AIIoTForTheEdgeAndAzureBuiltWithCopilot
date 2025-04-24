using System;
using System.Linq;
using System.IO;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ResnetV5ObjectClassificationApplication
{
   class Program
   {
      static void Main()
      {
         string modelPath = "resnet50-v2-7.onnx"; // Updated model path
         string imagePath = "pizza.jpg"; // Updated image path
         string labelsPath = "labels.txt"; // Path to labels file

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

         // Load labels
         var labels = File.ReadAllLines(labelsPath);

         // Find Top 10 labels and their confidence scores
         var top10 = probabilities
             .Select((prob, index) => new { Label = labels[index], Confidence = prob })
             .OrderByDescending(item => item.Confidence)
             .Take(10);

         Console.WriteLine("Top 10 Predictions:");
         foreach (var item in top10)
         {
            Console.WriteLine($"{item.Label}: {item.Confidence:F4}");
         }

         Console.WriteLine("Press ENTER to exit");
         Console.ReadLine();
      }

      static DenseTensor<float> LoadAndPreprocessImage(string imagePath)
      {
         int width = 224, height = 224; // ResNet50 expects 224x224 input

         using var image = Image.Load<Rgb24>(imagePath);
         image.Mutate(x => x.Resize(width, height));

         var tensor = new DenseTensor<float>(new[] { 1, 3, width, height });

         // ImageNet mean & standard deviation values
         float[] mean = { 0.485f, 0.456f, 0.406f };
         float[] stdev = { 0.229f, 0.224f, 0.225f };

         for (int y = 0; y < height; y++)
         {
            for (int x = 0; x < width; x++)
            {
               var pixel = image[x, y];

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
         float maxVal = logits.Max();
         var expScores = logits.Select(logit => Math.Exp(logit - maxVal)).ToArray(); // Explicitly cast logit to double  
         double sumExpScores = expScores.Sum();
         return expScores.Select(score => (float)(score / sumExpScores)).ToArray();
      }
   }
}