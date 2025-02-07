// Start with SecurityCameraHttpClient code
// Use a stream rather than loading from a file
// Use YoloSharp to run an onnx Object Detection model on the image
// get onnx model path from application settings
// Save image if object with specified name detected
// Modify so objectDetected supports multiple images names.
// Make logging of detections configurable in app settings
// Make saving of image configurable in app settings
// Modify HttpClientHandler initialisation to pre authenticate with network credentials - tried multiple different ways could get it to work
// Modify code to make use of GPU configurable - to hard

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
         _yoloModel = new YoloPredictor(_applicationSettings.OnnxModelPath, new YoloPredictorOptions 
         { 
            UseCuda = _applicationSettings.UseCuda,
         });

         using (HttpClientHandler handler = new HttpClientHandler { Credentials = new NetworkCredential(_applicationSettings.Username, _applicationSettings.Password), PreAuthenticate = true })
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

            Console.WriteLine($"{DateTime.UtcNow:yy-MM-dd HH:mm:ss.fff} SecurityCameraClient download done");

            using (var imageStream = await response.Content.ReadAsStreamAsync())
            {
               // Run object detection on the image stream
               Console.WriteLine($"{DateTime.UtcNow:yy-MM-dd HH:mm:ss.fff} Yolo detect starting");
               var detections = _yoloModel.Detect(imageStream);
               Console.WriteLine($"{DateTime.UtcNow:yy-MM-dd HH:mm:ss.fff} YoloDetect done");

               // Log all detections if logging is enabled
               if (_applicationSettings.LogDetections)
               {
                  foreach (var detection in detections)
                  {
                     Console.WriteLine($"Detected {detection.Name.Name} with confidence {detection.Confidence}");
                  }
               }

               // Check if any detection matches the specified object names
               bool objectDetected = detections.Any(d => _applicationSettings.ObjectNames.Contains(d.Name.Name));

               if (objectDetected && _applicationSettings.SaveImage)
               {
                  // Save the image if the specified object is detected and saving is enabled
                  string savePath = string.Format(_applicationSettings.SavePath, DateTime.UtcNow);

                  using (var fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None))
                  {
                     imageStream.Position = 0;

                     await imageStream.CopyToAsync(fileStream);
                  }
               }
            }
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
      public required List<string> ObjectNames { get; set; }
      public bool LogDetections { get; set; } = false; // Add LogDetections property
      public bool SaveImage { get; set; } = false; // Add SaveImage property
      public bool UseCuda { get; set; } = false;
   }
}
