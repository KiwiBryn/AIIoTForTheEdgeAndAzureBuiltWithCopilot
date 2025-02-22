using Microsoft.Extensions.Configuration;

using Compunet.YoloSharp;

using Nager.VideoStream;


namespace SecurityCameraRTSPClientNagerVideoStream
{
   internal class Program
   {
      private static ApplicationSettings _applicationSettings;
      private static YoloPredictor _yolo;

      static void Main(string[] args)
      {
         Console.WriteLine($"{DateTime.UtcNow:yy-MM-dd HH:mm:ss} SecurityCameraRTSPClientNagerVideoStream");

         try
         {
            // load the app settings into configuration
            var configuration = new ConfigurationBuilder()
                 .AddJsonFile("appsettings.json", false, true)
            .AddUserSecrets<Program>()
            .Build();

            _applicationSettings = configuration.GetSection("ApplicationSettings").Get<ApplicationSettings>();

            if (!Directory.Exists(_applicationSettings.SavePath))
            {
               Directory.CreateDirectory(_applicationSettings.SavePath);
            }

            var inputSource = new StreamInputSource(_applicationSettings.RtspCameraUrl);

            var cancellationTokenSource = new CancellationTokenSource();

            using (_yolo = new YoloPredictor(_applicationSettings.ModelPath, new YoloPredictorOptions()
            {
               Configuration = new YoloConfiguration()
               {
                  Confidence = 0.75f,
                  SuppressParallelInference = true,
               }
            }))
            {
               File.WriteAllText($"{_applicationSettings.SavePath}\\{DateTime.Now:yyMMdd-HHmmss-fff}-start.txt", "Starting");

               _ = Task.Run(async () => await StartStreamProcessingAsync(inputSource, cancellationTokenSource.Token));

               Console.WriteLine("Press any key to stop");
               Console.ReadKey();
               File.WriteAllText($"{_applicationSettings.SavePath}\\{DateTime.Now:yyMMdd-HHmmss-fff}-stop.txt", "Stopping");

               cancellationTokenSource.Cancel();

               Console.WriteLine("Press ENTER to exit");
               Console.ReadLine();
            }
         }
         catch (Exception ex)
         {
            Console.WriteLine($"{DateTime.UtcNow:yy-MM-dd HH:mm:ss} Application shutdown failure {ex.Message}", ex);
         }
      }

      private static async Task StartStreamProcessingAsync(InputSource inputSource, CancellationToken cancellationToken = default)
      {
         Console.WriteLine("Start Stream Processing");

         try
         {
            var client = new VideoStreamClient();

            client.NewImageReceived += NewImageReceived;

#if FFMPEG_INFO_DISPLAY
            client.FFmpegInfoReceived += FFmpegInfoReceived;
#endif
            await client.StartFrameReaderAsync(inputSource, OutputImageFormat.Png, cancellationToken: cancellationToken);

#if FFMPEG_INFO_DISPLAY
            client.FFmpegInfoReceived -= FFmpegInfoReceived;
#endif
            Console.WriteLine("End Stream Processing");
         }
         catch (Exception exception)
         {
            Console.WriteLine($"{exception}");
         }
      }

      static DateTime LastTimeUtc = DateTime.UtcNow;

      private static void NewImageReceived(byte[] imageData)
      {
         //Console.WriteLine($"{DateTime.UtcNow:yy-MM-dd HH:mm:ss.fff} New image received, bytes:{imageData.Length}");

         var start = DateTime.UtcNow;

         var results = _yolo.Detect(imageData);

         //foreach (var result in results)
         //{
         //   Console.WriteLine($"Name: {result.Name.Name} Confidence:{result.Confidence} Bounding Box{result.Bounds}");
         //}

         if (results.Count > 0)
         {
            string outputPath = Path.Combine(_applicationSettings.SavePath, string.Format(_applicationSettings.FrameFileNameFormat, DateTime.UtcNow));

            File.WriteAllBytes(outputPath, imageData);
         }

         Console.WriteLine($"{DateTime.UtcNow:yy-MM-dd HH:mm:ss.fff} LastTime:{(DateTime.UtcNow - LastTimeUtc).TotalMilliseconds:0} mSec Duration:{(DateTime.UtcNow - start).TotalMilliseconds:0} mSec");
         LastTimeUtc = DateTime.UtcNow;
      }

#if FFMPEG_INFO_DISPLAY
      private static void FFmpegInfoReceived(string ffmpegStreamInfo)
      {
         Console.WriteLine(ffmpegStreamInfo);
      }
#endif

   }

   public class ApplicationSettings
   {
      public string RtspCameraUrl { get; set; }

      public string SavePath { get; set; } = "";

      public string FrameFileNameFormat { get; set; } = "";

      public string ModelPath { get; set; }
   }
}

