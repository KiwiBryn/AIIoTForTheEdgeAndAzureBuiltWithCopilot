// please write a C# console application that uses Onnx to run a ResNet50 object detection model on an image loaded from disk
// Modify to use imagesharp
// Resize image to 224x224 and crop
// Load image labels from a file
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
            Mode = ResizeMode.Crop
            //Mode = ResizeMode.BoxPad
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

         // Process the results
         var output = results.First().AsEnumerable<float>().ToArray();
         var predictedLabelIndex = output.ToList().IndexOf(output.Max());

         // Load labels
         var labels = File.ReadAllLines(labelsPath);
         var predictedLabel = labels[predictedLabelIndex];

         Console.WriteLine($"Predicted Label: {predictedLabel}");

         Console.WriteLine("Press ENTER to exit");
         Console.ReadLine();
      }

      private static DenseTensor<float> ImageToTensor(Image<Rgb24> image)
      {
         int width = image.Width;
         int height = image.Height;
         var tensor = new DenseTensor<float>(new[] { 1, 3, height, width });

         image.ProcessPixelRows(accessor =>
         {
            for (int y = 0; y < height; y++)
            {
               var pixelRow = accessor.GetRowSpan(y);
               for (int x = 0; x < width; x++)
               {
                  tensor[0, 0, y, x] = pixelRow[x].R / 255.0f;
                  tensor[0, 1, y, x] = pixelRow[x].G / 255.0f;
                  tensor[0, 2, y, x] = pixelRow[x].B / 255.0f;
               }
            }
         });

         return tensor;
      }
   }
}
