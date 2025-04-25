// please write an httpTrigger azure function that uses ResNet50v2  and ONNX to classify images uploaded in a form
// modify code to use ImageSharp
// Modify the code to load the Resnet50v27 classification labels from a file called labels.txt
// Modify the code so OkObjectResult returns JSON with the predicted label
// Modify the code so only one image can be uploaded
// add the confidence to the reposonse
//
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
      private readonly List<string> _labels;

      public Function1(ILogger<Function1> logger)
      {
         _logger = logger;
         _session = new InferenceSession("resnet50-v2-7.onnx");
         _labels = File.ReadAllLines("labels.txt").ToList();
      }

      [Function("ResnetV50ImageClassificationFunction")]
      public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
      {
         _logger.LogInformation("C# HTTP trigger function processed a request.");

         if (!req.HasFormContentType || req.Form.Files.Count != 1)
         {
            return new BadRequestObjectResult("Please upload exactly one image file.");
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

         var softmaxOutput = Softmax(results[0].AsEnumerable<float>().ToArray());

         // Process the results
         var top10 = softmaxOutput
            .Select((value, index) => new { Value = value, Index = index })
            .OrderByDescending(x => x.Value)
            .Take(10)
            .ToList();

         return new OkObjectResult(top10);
      }

      private DenseTensor<float> PreprocessImage(Image<Rgb24> image)
      {
         image.Mutate(x => x.Resize(new ResizeOptions // Fixed with intellisense
         {
            Size = new Size(224, 224),
            //Mode = ResizeMode.Crop
            Mode = ResizeMode.BoxPad
         }));

         // Mean and standard deviation values for normalization
         float[] mean = { 0.485f, 0.456f, 0.406f };
         float[] stddev = { 0.229f, 0.224f, 0.225f };

         var tensor = new DenseTensor<float>(new[] { 1, 3, 224, 224 });

         for (int y = 0; y < 224; y++)
         {
            for (int x = 0; x < 224; x++)
            {
               var pixel = image[x, y];
               tensor[0, 0, y, x] = (pixel.R / 255.0f - mean[0]) / stddev[0]; ;
               tensor[0, 1, y, x] = (pixel.G / 255.0f - mean[1]) / stddev[1]; ;
               tensor[0, 2, y, x] = (pixel.B / 255.0f - mean[2]) / stddev[2]; ;
            }
         }

         return tensor;
      }

      private static float[] Softmax(float[] values)
      {
         float maxVal = values.Max();
         float[] expValues = values.Select(v => (float)Math.Exp(v - maxVal)).ToArray();
         float sumExpValues = expValues.Sum();
         return expValues.Select(v => v / sumExpValues).ToArray();
      }
   }
}
