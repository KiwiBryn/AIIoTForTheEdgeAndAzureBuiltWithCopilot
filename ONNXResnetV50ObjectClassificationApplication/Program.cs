// please write a C# console application that uses Onnx to run a ResNet50 object detection model on an image loaded from disk
// Modify to use imagesharp
// Resize image to 224x224 and crop
// Load image labels from a file
// Add stdev & means
// Caclulate the softmax
// Calculate the Top 10 labels and their confidence
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
//using System.Drawing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ONNXResnetV5ObjectClassificationApplication
{
   internal class Program
   {
      static void Main(string[] args)
      {
         string modelPath = "..\\..\\..\\..\\Models\\resnet50-v2-7.onnx"; // Path to the ONNX model
         string imagePath = "pizza.jpg"; // Path to the input image
         string labelsPath = "labels.txt"; // Path to the labels file

         // Load the image
         using var image = Image.Load<Rgb24>(imagePath);
         image.Mutate(x => x.Resize(new ResizeOptions
         {
            Size = new Size(224, 224),
            Mode = ResizeMode.BoxPad
            //Mode = ResizeMode.Min
            //Mode = ResizeMode.Max
            //Mode = ResizeMode.Crop
            //Mode = ResizeMode.Stretch
         }));

         image.Save("..\\..\\..\\pizza-resized.jpeg");

         var inputTensor = ImageToTensor(image);

         // Load the ONNX model
         using var session = new InferenceSession(modelPath);

         // Create input tensor
         var inputMeta = session.InputMetadata;
         var inputs = new List<NamedOnnxValue>
                           {
                               NamedOnnxValue.CreateFromTensor("data", inputTensor)
                           };

         // Run inference
         using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = session.Run(inputs);


         var softmaxOutput = Softmax(results[0].AsEnumerable<float>().ToArray());

         // Process the results
         var output = results.First().AsEnumerable<float>().ToArray();
         var top10 = softmaxOutput
            .Select((value, index) => new { Value = value, Index = index })
            .OrderByDescending(x => x.Value)
            .Take(10)
            .ToList();

         // Load labels
         var labels = File.ReadAllLines(labelsPath);

         // Display top 10 labels and their confidence
         foreach (var item in top10)
         {
            Console.WriteLine($"Label: {labels[item.Index]}, Confidence: {item.Value}");
         }

         Console.WriteLine("Press ENTER to exit");
         Console.ReadLine();
      }

      private static DenseTensor<float> ImageToTensor(Image<Rgb24> image)
      {
         int width = image.Width;
         int height = image.Height;
         var tensor = new DenseTensor<float>(new[] { 1, 3, height, width });

         // Mean and standard deviation values for normalization
         float[] mean = { 0.485f, 0.456f, 0.406f };
         float[] stddev = { 0.229f, 0.224f, 0.225f };

         image.ProcessPixelRows(accessor =>
         {
            for (int y = 0; y < height; y++)
            {
               var pixelRow = accessor.GetRowSpan(y);
               for (int x = 0; x < width; x++)
               {
                  tensor[0, 0, y, x] = (pixelRow[x].R / 255.0f - mean[0]) / stddev[0];
                  tensor[0, 1, y, x] = (pixelRow[x].G / 255.0f - mean[1]) / stddev[1];
                  tensor[0, 2, y, x] = (pixelRow[x].B / 255.0f - mean[2]) / stddev[2];
               }
            }
         });

         return tensor;
      }
      // Calculate the softmax
      private static float[] Softmax(float[] values)
      {
         float maxVal = values.Max();
         float[] expValues = values.Select(v => (float)Math.Exp(v - maxVal)).ToArray();
         float sumExpValues = expValues.Sum();
         return expValues.Select(v => v / sumExpValues).ToArray();
      }
   }
}
