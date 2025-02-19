// please write an httpTrigger azure function that uses ResNet50v2  and ONNX to classify images uploaded in a form
// modify code to use ImageSharp

//using System.Drawing; // Added with intelisense, then removed when NuGet removed
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

using Microsoft.ML.OnnxRuntime; // Added with intelisense after adding NuGet package
using Microsoft.ML.OnnxRuntime.Tensors;

using SixLabors.ImageSharp;// Added with intelisense after adding NuGet package
using SixLabors.ImageSharp.PixelFormats; // Added with intelisense
using SixLabors.ImageSharp.Processing; // Added with intelisense


namespace ONNXResnet50v27
{
   public class Function1
   {
      private readonly ILogger<Function1> _logger;
      private readonly InferenceSession _session;

      public Function1(ILogger<Function1> logger)
      {
         _logger = logger;
         _session = new InferenceSession("resnet50-v2-7.onnx");
      }

      [Function("ImageClassificationFunction")]
      public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
      {
         _logger.LogInformation("C# HTTP trigger function processed a request.");

         if (!req.HasFormContentType || !req.Form.Files.Any())
         {
            return new BadRequestObjectResult("Please upload an image file.");
         }

         var file = req.Form.Files[0];
         if (file.Length == 0)
         {
            return new BadRequestObjectResult("The uploaded file is empty.");
         }

         using var stream = file.OpenReadStream();
         using var image = await Image.LoadAsync<Rgb24>(stream);  // Fixed with intellisense
         var input = PreprocessImage(image);

         var inputs = new List<NamedOnnxValue>
                  {
                      NamedOnnxValue.CreateFromTensor("data", input)
                  };

         using var results = _session.Run(inputs);
         var output = results.First().AsEnumerable<float>().ToArray();
         var predictedLabel = output.ToList().IndexOf(output.Max());

         return new OkObjectResult($"Predicted label: {predictedLabel}");
      }

      private DenseTensor<float> PreprocessImage(Image<Rgb24> image)
      {
         image.Mutate(x => x.Resize(new ResizeOptions // Fixed with intellisense
         {
            Size = new Size(224, 224),
            Mode = ResizeMode.Crop
         }));

         var tensor = new DenseTensor<float>(new[] { 1, 3, 224, 224 });

         for (int y = 0; y < 224; y++)
         {
            for (int x = 0; x < 224; x++)
            {
               var pixel = image[x, y];
               tensor[0, 0, y, x] = pixel.R / 255.0f;
               tensor[0, 1, y, x] = pixel.G / 255.0f;
               tensor[0, 2, y, x] = pixel.B / 255.0f;
            }
         }

         return tensor;
      }
   }
}
