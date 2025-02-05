// Start with SecurityCameraHttpClient code
// Use a stream rather than loading from a file
// Use YoloSharp to run an onnx Object Detection model on the image
// get onnx model path from application settings
// Save image if object with specified name detected
// Modify to images names.
// Log detections even if no objectDetected 
using System.Net;

using Microsoft.Extensions.Configuration;

using Compunet.YoloSharp;

namespace SecurityCameraHttpClientYoloSharpObjectDetection
{
   internal class Program
   {
      private static HttpClient _client;
      private static bool _isRetrievingImage = false;
      private static ApplicationSettings _applicationSettings;
      private static YoloPredictor _yoloModel; // Add YoloModel field

      static void Main(string[] args)
      {
         Console.WriteLine($"{DateTime.UtcNow:yy-MM-dd HH:mm:ss} SecurityCameraClient starting");
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

         // Initialize YoloModel with path from application settings
         _yoloModel = new YoloPredictor(_applicationSettings.OnnxModelPath);

         using (HttpClientHandler handler = new HttpClientHandler { Credentials = new NetworkCredential(_applicationSettings.Username, _applicationSettings.Password) })
         using (_client = new HttpClient(handler))
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
            Console.WriteLine($"{DateTime.UtcNow:yy-MM-dd HH:mm:ss.fff} SecurityCameraClient download starting");

            HttpResponseMessage response = await _client.GetAsync(_applicationSettings.CameraUrl);
            response.EnsureSuccessStatusCode();

            using (var imageStream = await response.Content.ReadAsStreamAsync())
            {
               // Run object detection on the image stream
               var detections = _yoloModel.Detect(imageStream);

               // Log all detections
               foreach (var detection in detections)
               {
                  Console.WriteLine($"Detected {detection.Name.Name} with confidence {detection.Confidence}");
               }

               // Check if any detection matches the specified object name
               bool objectDetected = detections.Any(d => d.Name.Name == _applicationSettings.SpecifiedObjectName);

               if (objectDetected)
               {
                  // Save the image if the specified object is detected
                  string savePath = string.Format(_applicationSettings.SavePath, DateTime.UtcNow);
                  using (var fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None))
                  {
                     await imageStream.CopyToAsync(fileStream);
                  }
               }
            }

            Console.WriteLine($"{DateTime.UtcNow:yy-MM-dd HH:mm:ss.fff} SecurityCameraClient download done");
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
      public string OnnxModelPath { get; set; } = ""; // Add OnnxModelPath property
      public string SpecifiedObjectName { get; set; } = ""; // Add SpecifiedObjectName property
   }
}
