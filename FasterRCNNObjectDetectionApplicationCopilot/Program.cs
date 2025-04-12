// Added Microsoft.ML.OnnxRuntime; & SixLabors.ImageSharp NuGet packages
//using System;
//using System.Collections.Generic;
//using System.Linq;
using Microsoft.ML.OnnxRuntime; 
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp; 
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;


namespace FasterRCNNObjectDetectionApplicationCopilot
{
   class Program
   {
      static void Main(string[] args)
      {
         // Paths to the ONNX model and the image file (adjust these paths as needed)
         string modelPath = @"..\\..\\..\\..\\Models\\FasterRCNN-10.onnx";
         string imagePath = "sports.jpg";

         // Create the OnnxInference session
         using var session = new InferenceSession(modelPath);

         // Load the image from disk using ImageSharp
         using var image = Image.Load<Rgb24>(imagePath);

         // Resize the image to fit within the range and adjust dimensions to be divisible by 32
         ResizeImage(image);

         // Extract tensor data from the image (with shape [3, height, width])
         var inputTensor = ExtractTensorFromImage(image);

         // Create NamedOnnxValue input (ensure that the input name "image" matches your model's input)
         var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("image", inputTensor)
            };

         // Run the model inference  
         using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = session.Run(inputs);

         // Process and display the outputs (bounding boxes, labels, and confidences)
         ProcessOutput(results);

         Console.WriteLine("Press Enter to exit");
         Console.ReadLine();
      }

      /// <summary>
      /// Resizes the input image such that both width and height are within the range [800, 1333]
      /// and ensures the dimensions are divisible by 32.
      /// </summary>
      private static void ResizeImage(Image<Rgb24> image)
      {
         const int minSize = 800;
         const int maxSize = 1333;
         int originalWidth = image.Width;
         int originalHeight = image.Height;

         // Determine the scaling factor so that the smallest side is at least minSize and the largest does not exceed maxSize.
         float scale = Math.Min((float)maxSize / Math.Max(originalWidth, originalHeight),
                                (float)minSize / Math.Min(originalWidth, originalHeight));

         // Compute the new dimensions based on the scale
         int newWidth = (int)(originalWidth * scale);
         int newHeight = (int)(originalHeight * scale);

         // Adjust dimensions to be divisible by 32
         newWidth = (newWidth / 32) * 32;
         newHeight = (newHeight / 32) * 32;

         image.Mutate(x => x.Resize(newWidth, newHeight));
      }

      /// <summary>
      /// Converts the resized image into a DenseTensor&lt;float&gt; with shape [3, height, width].
      /// The image is processed to subtract the Faster‑RCNN channel means (B, G, R order).
      /// </summary>
      private static DenseTensor<float> ExtractTensorFromImage(Image<Rgb24> image)
      {
         int width = image.Width;
         int height = image.Height;

         // Create a tensor with shape [channels, height, width]
         var tensor = new DenseTensor<float>(new[] { 3, height, width });

         // Faster‑RCNN channel means (order: blue, green, red)
         float[] mean = { 102.9801f, 115.9465f, 122.7717f };

         // Process each pixel row; ImageSharp provides efficient pixel row access.
         image.ProcessPixelRows(accessor =>
         {
            for (int y = 0; y < height; y++)
            {
               var pixelRow = accessor.GetRowSpan(y);
               for (int x = 0; x < width; x++)
               {
                  // Subtract the channel mean value (ensuring B, G, R order)
                  tensor[0, y, x] = pixelRow[x].B - mean[0];
                  tensor[1, y, x] = pixelRow[x].G - mean[1];
                  tensor[2, y, x] = pixelRow[x].R - mean[2];
               }
            }
         });

         return tensor;
      }

      /// <summary>
      /// Processes the model output, extracting bounding boxes, labels, and confidences.
      /// Only detections with confidence scores above a defined threshold are printed.
      /// </summary>
      private static void ProcessOutput(IDisposableReadOnlyCollection<DisposableNamedOnnxValue> output)
      {
         // Note: The output node names ("6379", "6381", "6383") are model-specific.
         // They correspond, respectively, to boxes, labels, and confidence scores.
         var boxesTensor = output.First(x => x.Name == "6379").AsTensor<float>();
         var labelsTensor = output.First(x => x.Name == "6381").AsTensor<long>();
         var confidencesTensor = output.First(x => x.Name == "6383").AsTensor<float>();

         float[] boxes = boxesTensor.ToArray();
         long[] labels = labelsTensor.ToArray();
         float[] confidences = confidencesTensor.ToArray();

         const float minConfidence = 0.7f;

         // Each bounding box is represented by 4 values: x1, y1, x2, y2.
         for (int i = 0; i < boxes.Length; i += 4)
         {
            int detectionIndex = i / 4;
            if (confidences[detectionIndex] >= minConfidence)
            {
               long label = labels[detectionIndex];
               float confidence = confidences[detectionIndex];
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
