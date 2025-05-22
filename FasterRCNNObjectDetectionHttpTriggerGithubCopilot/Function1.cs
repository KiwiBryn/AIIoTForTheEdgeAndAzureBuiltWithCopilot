// please write an httpTrigger azure function that uses Faster RCNN and ONNX to detect the object in an image uploaded in the body of an HTTP Post
//    manually added the ML.Net ONNX NuGet + using directives
//    manually added the ImageSharp NuGet + using directives
//    Used Copilot to add Microsoft.ML.OnnxRuntime.Tensors using directive
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp; // Couldn't get inteliisense after adding NuGet package
using SixLabors.ImageSharp.PixelFormats; // Couldn't get inteliisense after adding NuGet package
using SixLabors.ImageSharp.Processing; // Couldn't get inteliisense after adding NuGet package


namespace FasterRCNNObjectDetectionHttpTriggerGithubCopilot
{
   public class Function1
   {
      private readonly ILogger<Function1> _logger;
      private readonly InferenceSession _session;
      private readonly List<string> _labels;

      public Function1(ILogger<Function1> logger)
      {
         _logger = logger;
         // Load ONNX model and labels
         _session = new InferenceSession("FasterRCNN-10.onnx");
         _labels = File.ReadAllLines("labels.txt").ToList();
      }

      [Function("ObjectDetectionFunction")]
      public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
      {
         if (!req.ContentType.StartsWith("image/"))
            return new BadRequestObjectResult("Content-Type must be an image.");

         using var ms = new MemoryStream();
         await req.Body.CopyToAsync(ms);
         ms.Position = 0;

         using var image = Image.Load<Rgb24>(ms);
         var inputTensor = PreprocessImage(image);

         var inputs = new List<NamedOnnxValue>
               {
                   NamedOnnxValue.CreateFromTensor("image", inputTensor)
               };

         using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = _session.Run(inputs);
         var output = results.ToDictionary(x => x.Name, x => x.Value);

         var boxes = (DenseTensor<float>)output["boxes"];
         var labels = (DenseTensor<long>)output["labels"];
         var scores = (DenseTensor<float>)output["scores"];

         var detections = new List<object>();
         for (int i = 0; i < scores.Length; i++)
         {
            if (scores[i] > 0.5)
            {
               detections.Add(new
               {
                  label = _labels[(int)labels[i]],
                  score = scores[i],
                  box = new
                  {
                     x1 = boxes[i, 0],
                     y1 = boxes[i, 1],
                     x2 = boxes[i, 2],
                     y2 = boxes[i, 3]
                  }
               });
            }
         }

         return new OkObjectResult(detections);
      }

      private static DenseTensor<float> PreprocessImage( Image<Rgb24> image)
      {
         // Resize to 800x800 (FasterRCNN default), normalize to [0,1], and convert to CHW
         const int targetWidth = 800;
         const int targetHeight = 800;
         image.Mutate(x => x.Resize(targetWidth, targetHeight));

         var tensor = new DenseTensor<float>(new[] { 1, 3, targetHeight, targetWidth });
         for (int y = 0; y < targetHeight; y++)
         {
            for (int x = 0; x < targetWidth; x++)
            {
               var pixel = image[x, y];
               tensor[0, 0, y, x] = pixel.R / 255f;
               tensor[0, 1, y, x] = pixel.G / 255f;
               tensor[0, 2, y, x] = pixel.B / 255f;
            }
         }
         return tensor;
      }
   }
}
