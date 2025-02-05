// Use a stream rather than loading from a file
// Use YoloDotNet to run an onnx Object Detection model on the image
// Configure YOLO With YoloOptions became Configure YOLO creation with YoloOptions 
// Please check YoloOptions properties badly wrong
// Make cuda in YoloOptions configurable
// This one was a dumpster file seemed be getting confused with EMGU and https://github.com/BobLd
// Make object detection confidence configurable
using System.Net;

using Microsoft.Extensions.Configuration;
using SkiaSharp;
using YoloDotNet;
using YoloDotNet.Enums;
using YoloDotNet.Models;

namespace SecurityCameraHttpClientYoloDotNetObjectDetection
{
   internal class Program
   {
      private static HttpClient _client;
      private static bool _isRetrievingImage = false;
      private static ApplicationSettings _applicationSettings;
      private static Yolo _yolo;

      static void Main(string[] args)
      {
         Console.WriteLine($"{DateTime.UtcNow:yy-MM-dd HH:mm:ss} SecurityCameraHttpClientYoloDotNetObjectDetection starting");
#if RELEASE
            Console.WriteLine("RELEASE");
#else
         Console.WriteLine("DEBUG");
#endif

         var configuration = new ConfigurationBuilder()
              .AddJsonFile("appsettings.json", false, true)
         .AddUserSecrets<Program>()
         .Build();

         _applicationSettings = configuration.GetSection("ApplicationSettings").Get<ApplicationSettings>();

         using (HttpClientHandler handler = new HttpClientHandler { Credentials = new NetworkCredential(_applicationSettings.Username, _applicationSettings.Password) })
         using (_client = new HttpClient(handler))
         using (_yolo = new Yolo(new YoloOptions()
         {
            OnnxModel = _applicationSettings.OnnxModelPath,
            ModelType = ModelType.ObjectDetection,
            Cuda = _applicationSettings.UseCuda // Blew up as default CUDA enabled
         }))
         using (var timer = new Timer(async _ => await RetrieveImageAsync(), null, _applicationSettings.TimerDue, _applicationSettings.TimerPeriod))
         {
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
         }
      }

      private static async Task RetrieveImageAsync()
      {
         if (_isRetrievingImage) return;

         _isRetrievingImage = true;
         try
         {
            Console.WriteLine($"{DateTime.UtcNow:yy-MM-dd HH:mm:ss.fff} download starting");

            HttpResponseMessage response = await _client.GetAsync(_applicationSettings.CameraUrl);
            response.EnsureSuccessStatusCode();

            using (Stream imageStream = await response.Content.ReadAsStreamAsync())
            {
               string savePath = string.Format(_applicationSettings.SavePath, DateTime.UtcNow);
               using (FileStream fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None))
               {
                  await imageStream.CopyToAsync(fileStream);
               }

               // Run object detection
               //var items = yolo.Detect(imageStream);
               var items = new List<ObjectDetection>();

               var image = SKImage.FromEncodedData(savePath);
               {
                  Console.WriteLine($"{DateTime.UtcNow:yy-MM-dd HH:mm:ss.ffff} detect starting");

                  items = _yolo.RunObjectDetection(image);

                  Console.WriteLine($"{DateTime.UtcNow:yy-MM-dd HH:mm:ss.ffff} detect done");
               }

               foreach (var item in items)
               {
                  if (item.Confidence >= _applicationSettings.ConfidenceThreshold)
                  {
                     Console.WriteLine($"Detected {item.Label.Name} with confidence {item.Confidence} at location {item.BoundingBox}");
                  }
               }
            }

            Console.WriteLine($"{DateTime.UtcNow:yy-MM-dd HH:mm:ss.fff} download done");
         }
         catch (Exception ex)
         {
            Console.WriteLine($"An error occurred: {ex.Message}");
         }
         finally
         {
            _isRetrievingImage = false;
         }
      }
   }

   public class ApplicationSettings
   {
      public string CameraUrl { get; set; } = "";

      public string SavePath { get; set; } = "";

      public string Username { get; set; } = "";

      public string Password { get; set; } = "";

      public TimeSpan TimerDue { get; set; } = TimeSpan.Zero;

      public TimeSpan TimerPeriod { get; set; } = TimeSpan.Zero;

      public string OnnxModelPath { get; set; } = "";

      public bool UseCuda { get; set; } = false;

      public float ConfidenceThreshold { get; set; } = 0.5f; // Default confidence threshold
   }
}
