// please write a C# console application that uses Onnx to run a ONNX Faster-RCNN object detection model on an image loaded from disk
//    Added System.Drawing.Common Microsoft.ML.OnnxRuntime NuGet package
// Fixed up paths to the model and image.
// please fix "Invalid rank for input: image Got: 4 Expected: 3 Please fix either the inputs/outputs or the model"
// change dimensions of declaration tensor rather than reshaping it
// change from System.Drawing to ImageSharp 
//    Added ImageSharp NuGet, removed System.Drawing.Common NuGet package
// Use ImageSharp to resize the image such that both height and width are within the range of [800, 1333], such that both height and width are divisible by 32.
// https://github.com/onnx/models/tree/main/validated/vision/object_detection_segmentation/faster-rcnn#preprocessing-steps
// Apply mean to each channel
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;


namespace ONNXFasterRCNNObjectDetectionApplication
{
   internal class Program
   {
      static void Main(string[] args)
      {
         string modelPath = "..\\..\\..\\..\\Models\\FasterRCNN-10.onnx";
         string imagePath = "sports.jpg";

         using var session = new InferenceSession(modelPath);
         using var image = Image.Load<Rgb24>(imagePath);

         // Resize image to fit within [800, 1333] and be divisible by 32
         ResizeImage(image);

         var inputTensor = ExtractTensorFromImage(image);
         var inputs = new List<NamedOnnxValue>
                     {
                         NamedOnnxValue.CreateFromTensor("image", inputTensor)
                     };

         using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = session.Run(inputs);
         var output = results.First().AsEnumerable<float>().ToArray();

         // Process the output (e.g., draw bounding boxes on the image)
         ProcessOutput(output, image);

         image.Save("output.jpg");
         Console.WriteLine("Object detection completed. Output saved as output.jpg");
      }

      private static void ResizeImage(Image<Rgb24> image)
      {
         const int minSize = 800;
         const int maxSize = 1333;

         int originalWidth = image.Width;
         int originalHeight = image.Height;

         float scale = Math.Min((float)maxSize / Math.Max(originalWidth, originalHeight), (float)minSize / Math.Min(originalWidth, originalHeight));
         int newWidth = (int)(originalWidth * scale);
         int newHeight = (int)(originalHeight * scale);

         // Ensure dimensions are divisible by 32
         newWidth = (newWidth / 32) * 32;
         newHeight = (newHeight / 32) * 32;

         image.Mutate(x => x.Resize(newWidth, newHeight));
      }

      private static DenseTensor<float> ExtractTensorFromImage(Image<Rgb24> image)
      {
         int width = image.Width;
         int height = image.Height;
         var tensor = new DenseTensor<float>(new[] { 3, height, width });

         // Mean values for each channel
         float[] mean = { 0.485f, 0.456f, 0.406f };

         image.ProcessPixelRows(accessor =>
         {
            for (int y = 0; y < height; y++)
            {
               var pixelRow = accessor.GetRowSpan(y);
               for (int x = 0; x < width; x++)
               {
                  tensor[0, y, x] = (pixelRow[x].R / 255.0f) - mean[0];
                  tensor[1, y, x] = (pixelRow[x].G / 255.0f) - mean[1];
                  tensor[2, y, x] = (pixelRow[x].B / 255.0f) - mean[2];
               }
            }
         });

         return tensor;
      }

      private static void ProcessOutput(float[] output, Image<Rgb24> image)
      {
         // Assuming the output format is [label, confidence, x1, y1, x2, y2]
         for (int i = 0; i < output.Length; i += 6)
         {
            int label = (int)output[i];
            float confidence = output[i + 1];
            float x1 = output[i + 2];
            float y1 = output[i + 3];
            float x2 = output[i + 4];
            float y2 = output[i + 5];

            Console.WriteLine($"Label: {label}, Confidence: {confidence}, Bounding Box: [{x1}, {y1}, {x2}, {y2}]");
         }
      }
   }
}
