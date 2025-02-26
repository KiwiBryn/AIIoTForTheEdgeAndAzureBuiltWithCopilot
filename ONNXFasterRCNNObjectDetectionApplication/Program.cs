// please write a C# console application that uses Onnx to run a ONNX Faster-RCNN object detection model on an image loaded from disk
//    Added System.Drawing.Common Microsoft.ML.OnnxRuntime NuGet package
// Fixed up paths to the model and image.
// please fix "Invalid rank for input: image Got: 4 Expected: 3 Please fix either the inputs/outputs or the model"
// change dimensions of declaration tensor rather than reshaping it

using System.Drawing;
using System.Drawing.Imaging;

using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;


namespace ONNXFasterRCNNObjectDetectionApplication
{
   internal class Program
   {
      static void Main(string[] args)
      {
         string modelPath = "..\\..\\..\\..\\Models\\FasterRCNN-10.onnx";
         string imagePath = "sports.jpg";

         using var session = new InferenceSession(modelPath);
         using var image = new Bitmap(imagePath);

         var inputTensor = ExtractTensorFromImage(image);
         var inputs = new List<NamedOnnxValue>
                  {
                      NamedOnnxValue.CreateFromTensor("image", inputTensor)
                  };

         using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = session.Run(inputs);
         var output = results.First().AsEnumerable<float>().ToArray();

         // Process the output (e.g., draw bounding boxes on the image)
         ProcessOutput(output, image);

         image.Save("output.jpg", ImageFormat.Jpeg);
         Console.WriteLine("Object detection completed. Output saved as output.jpg");
      }

      private static DenseTensor<float> ExtractTensorFromImage(Bitmap image)
      {
         int width = image.Width;
         int height = image.Height;
         var tensor = new DenseTensor<float>(new[] { 3, height, width });

         for (int y = 0; y < height; y++)
         {
            for (int x = 0; x < width; x++)
            {
               Color pixel = image.GetPixel(x, y);
               tensor[0, y, x] = pixel.R / 255.0f;
               tensor[1, y, x] = pixel.G / 255.0f;
               tensor[2, y, x] = pixel.B / 255.0f;
            }
         }

         return tensor;
      }

      private static void ProcessOutput(float[] output, Bitmap image)
      {
         // Implement the logic to process the output and draw bounding boxes on the image
         // This is a placeholder for the actual implementation
      }
   }
}
