using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Newtonsoft.Json;

public static class ResNet50Function
{
   private static readonly InferenceSession session = new InferenceSession("resnet50.onnx");

   [FunctionName("ImageClassification")]
   public static IActionResult Run(
       [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req,
       ILogger log)
   {
      log.LogInformation("Processing image classification request...");

      try
      {
         using var ms = new MemoryStream();
         req.Body.CopyTo(ms);
         using var image = Image.FromStream(ms);

         var inputTensor = PreprocessImage(image);

         var inputName = session.InputMetadata.Keys.First();
         var outputName = session.OutputMetadata.Keys.First();
         var result = session.Run(new Dictionary<string, NamedOnnxValue>
            {
                { inputName, NamedOnnxValue.CreateFromTensor(inputName, inputTensor) }
            });

         var predictions = result.First().AsTensor<float>().ToArray();

         return new JsonResult(new { predictions });
      }
      catch (Exception ex)
      {
         log.LogError($"Error: {ex.Message}");
         return new BadRequestObjectResult("Invalid image or request.");
      }
   }

   private static Tensor<float> PreprocessImage(Image image)
   {
      var resized = new Bitmap(image, new Size(224, 224));
      var tensorData = new float[1 * 3 * 224 * 224];

      for (int y = 0; y < 224; y++)
      {
         for (int x = 0; x < 224; x++)
         {
            var pixel = resized.GetPixel(x, y);
            tensorData[(0 * 3 * 224 * 224) + (0 * 224 * 224) + (y * 224) + x] = pixel.R / 255.0f;
            tensorData[(0 * 3 * 224 * 224) + (1 * 224 * 224) + (y * 224) + x] = pixel.G / 255.0f;
            tensorData[(0 * 3 * 224 * 224) + (2 * 224 * 224) + (y * 224) + x] = pixel.B / 255.0f;
         }
      }

      return new DenseTensor<float>(tensorData, new[] { 1, 3, 224, 224 });
   }
}
