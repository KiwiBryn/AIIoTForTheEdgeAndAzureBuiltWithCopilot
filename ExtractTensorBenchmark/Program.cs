using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ExtractTensorBenchmark
{
   internal class Program
   {
      static void Main(string[] args)
      {
         var summary = BenchmarkRunner.Run<ExtractTensor>();

         Console.WriteLine("Press Enter to exit");
         Console.ReadLine();
      }
   }


   public class ExtractTensor
   {
      private readonly Image<Rgb24> image;
      private Tensor<float> input;
      private List<NamedOnnxValue> inputs;
      private IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results;


      public ExtractTensor()
      {
         image = Image.Load<Rgb24>("sports.jpg");
      }

      [Benchmark]
      public void ResizeImage()
      {
         float ratio = 800f / Math.Min(image.Width, image.Height);

         image.Mutate(x => x.Resize((int)(ratio * image.Width), (int)(ratio * image.Height)));
      }

      [Benchmark]
      public void PreproccessImage()
      {
         var paddedHeight = (int)(Math.Ceiling(image.Height / 32f) * 32f);
         var paddedWidth = (int)(Math.Ceiling(image.Width / 32f) * 32f);

         Tensor<float> input = new DenseTensor<float>(new[] { 3, paddedHeight, paddedWidth });

         var mean = new[] { 102.9801f, 115.9465f, 122.7717f };

         image.ProcessPixelRows(accessor =>
         {
            for (int y = paddedHeight - accessor.Height; y < accessor.Height; y++)
            {
               Span<Rgb24> pixelSpan = accessor.GetRowSpan(y);
               for (int x = paddedWidth - accessor.Width; x < accessor.Width; x++)
               {
                  input[0, y, x] = pixelSpan[x].B - mean[0];
                  input[1, y, x] = pixelSpan[x].G - mean[1];
                  input[2, y, x] = pixelSpan[x].R - mean[2];
               }
            }
         });
      }

      [Benchmark]
      public void SetupInputsAndOutputs()
      {
         // Setup inputs and outputs
         inputs = new List<NamedOnnxValue>
         {
             NamedOnnxValue.CreateFromTensor("image", input)
         };
      }


      [Benchmark]
      public void RunInference()
      {
         using (var session = new InferenceSession("FasterRCNN-10.onnx", new SessionOptions()
         {
            EnableProfiling = true,
            //GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_EXTENDED,
            //ExecutionMode = ExecutionMode.ORT_SEQUENTIAL,
         }))
         {
            // Run inference
            //results = session.Run(inputs);
         }
      }

      [Benchmark]
      public void DenseTensorOnnxAiSample()
      {
         // Adapted from the ONNX Runtime C# sample for FasterRCNN
         // https://github.com/microsoft/onnxruntime/blob/main/csharp/sample/Microsoft.ML.OnnxRuntime.FasterRcnnSample/Program.cs

         var tensor = DenseTensorOnnxAiSample(image);
      }

      public DenseTensor<float> DenseTensorOnnxAiSample(Image<Rgb24> image)
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
   }
}