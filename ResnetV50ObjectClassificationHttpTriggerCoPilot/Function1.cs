using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
//using Microsoft.Azure.WebJobs;
//using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
//using Newtonsoft.Json;
using Microsoft.Azure.Functions.Worker;

public static class Function1
{
   private static readonly ILogger logger;
   private static readonly InferenceSession session = new InferenceSession("resnet50-v2-7.onnx");

   // Static constructor to initialize logger
   static Function1()
   {
      var loggerFactory = LoggerFactory.Create(builder =>
      {
         builder.AddConsole();
      });
      logger = loggerFactory.CreateLogger("Function1Logger");
   }

   [Function("ImageClassification")]
   public static async Task<IActionResult> Run(
       [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
   {
      logger.LogInformation("Processing image classification request...");

      try
      {
         using var ms = new MemoryStream();
         await req.Body.CopyToAsync(ms);
         using var image = Image.FromStream(ms);

         var inputTensor = PreprocessImage(image);

         var inputName = session.InputMetadata.Keys.First();
         var outputName = session.OutputMetadata.Keys.First();
         var inputList = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor(inputName, inputTensor)
            };

         var result = await Task.Run(() => session.Run(inputList));

         var predictions = result.First().AsTensor<float>().ToArray();

         return new JsonResult(new { predictions });
      }
      catch (Exception ex)
      {
         logger.LogError($"Error: {ex.Message}");
         return new BadRequestObjectResult("Invalid image or request.");
      }
   }

   private static Tensor<float> PreprocessImage(Image image)
   {
      var resized = new Bitmap(image, new Size(224, 224));
      var tensorData = new float[1 * 3 * 224 * 224];

      float[] mean = { 0.485f, 0.456f, 0.406f };
      float[] std = { 0.229f, 0.224f, 0.225f };

      for (int y = 0; y < 224; y++)
      {
         for (int x = 0; x < 224; x++)
         {
            var pixel = resized.GetPixel(x, y);

            tensorData[(0 * 3 * 224 * 224) + (0 * 224 * 224) + (y * 224) + x] = (pixel.R / 255.0f - mean[0]) / std[0];
            tensorData[(0 * 3 * 224 * 224) + (1 * 224 * 224) + (y * 224) + x] = (pixel.G / 255.0f - mean[1]) / std[1];
            tensorData[(0 * 3 * 224 * 224) + (2 * 224 * 224) + (y * 224) + x] = (pixel.B / 255.0f - mean[2]) / std[2];
         }
      }

      return new DenseTensor<float>(tensorData, new[] { 1, 3, 224, 224 });
   }
}
