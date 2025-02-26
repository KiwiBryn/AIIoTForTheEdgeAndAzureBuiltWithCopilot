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
// Use FasterRCNN means not resnet ones
// Change to B,G,R
// Display label, confidence and bounding box
// Get Copilot to fix
// Used Netron to fix up names
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

         // Process the output (e.g., draw bounding boxes on the image)
         ProcessOutput(results, image);

         image.Save("output.jpg");
         Console.WriteLine("Object detection completed. Output saved as output.jpg");

         Console.WriteLine("Press Enter to exit");
         Console.ReadLine();
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

         // Mean values for each channel for FasterRCNN
         float[] mean = { 102.9801f, 115.9465f, 122.7717f };

         image.ProcessPixelRows(accessor =>
         {
            for (int y = 0; y < height; y++)
            {
               var pixelRow = accessor.GetRowSpan(y);
               for (int x = 0; x < width; x++)
               {
                  tensor[0, y, x] = pixelRow[x].B - mean[0];
                  tensor[1, y, x] = pixelRow[x].G - mean[1];
                  tensor[2, y, x] = pixelRow[x].R - mean[2];
               }
            }
         });

         return tensor;
      }

      private static void ProcessOutput(IDisposableReadOnlyCollection<DisposableNamedOnnxValue> output, Image<Rgb24> image)
      {
         var boxes = output.First(x => x.Name == "6379").AsTensor<float>().ToArray();
         var labels = output.First(x => x.Name == "6381").AsTensor<long>().ToArray();
         var confidences = output.First(x => x.Name == "6383").AsTensor<float>().ToArray();

         const float minConfidence = 0.7f;

         for (int i = 0; i < boxes.Length; i += 4)
         {
            var index = i / 4;
            if (confidences[index] >= minConfidence)
            {
               long label = labels[index];
               float confidence = confidences[index];
               float x1 = boxes[i];
               float y1 = boxes[i + 1];
               float x2 = boxes[i + 2];
               float y2 = boxes[i + 3];

               Console.WriteLine($"Label: {label}, Confidence: {confidence}, Bounding Box: [{x1}, {y1}, {x2}, {y2}]");
            }
         }
      }
   }
}

