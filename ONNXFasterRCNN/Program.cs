// please write a C# console application that uses Onnx to run a ResNet50 object detection model on an image loaded from disk
// Had to add the following NuGet packages: System.Drawing.Common, Microsoft.ML.OnnxRuntime
using System.Drawing;

using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace ONNXFasterRCNN
{
   internal class Program
   {
      static void Main(string[] args)
      {
         string modelPath = "resnet50.onnx"; // Path to the ResNet50 ONNX model
         string imagePath = "image.jpg"; // Path to the image file

         // Load the image
         Bitmap bitmap = new Bitmap(imagePath);
         var inputTensor = ImageToTensor(bitmap);

         // Load the ONNX model
         using var session = new InferenceSession(modelPath);

         // Create input container
         var inputMeta = session.InputMetadata;
         var inputs = new List<NamedOnnxValue>
               {
                   NamedOnnxValue.CreateFromTensor("data", inputTensor)
               };

         // Run inference
         using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = session.Run(inputs);

         // Process the results
         foreach (var result in results)
         {
            Console.WriteLine($"{result.Name}: {result.AsTensor<float>().GetArrayString()}");
         }
      }

      private static DenseTensor<float> ImageToTensor(Bitmap bitmap)
      {
         int width = bitmap.Width;
         int height = bitmap.Height;
         var tensor = new DenseTensor<float>(new[] { 1, 3, height, width });

         for (int y = 0; y < height; y++)
         {
            for (int x = 0; x < width; x++)
            {
               Color color = bitmap.GetPixel(x, y);
               tensor[0, 0, y, x] = color.R / 255.0f;
               tensor[0, 1, y, x] = color.G / 255.0f;
               tensor[0, 2, y, x] = color.B / 255.0f;
            }
         }

         return tensor;
      }
   }
}
