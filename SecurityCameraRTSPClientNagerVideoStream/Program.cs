using Microsoft.Extensions.Configuration;

using Nager.VideoStream;


namespace SecurityCameraRTSPClientNagerVideoStream
{
   internal class Program
   {
      private static ApplicationSettings _applicationSettings;

      static void Main(string[] args)
      {
         Console.WriteLine($"{DateTime.UtcNow:yy-MM-dd HH:mm:ss} SecurityCameraRTSPClientNagerVideoStream");
#if RELEASE
         Console.WriteLine("RELEASE");
#else
         Console.WriteLine("DEBUG");
#endif

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

            Console.WriteLine($"{DateTime.UtcNow:yy-MM-dd HH:mm:ss.fff} Starting");

            _ = Task.Run(async () => await StartStreamProcessingAsync(inputSource, cancellationTokenSource.Token));

            Console.WriteLine("Press any key to stop");
            Console.ReadKey();

            Console.WriteLine($"{DateTime.UtcNow:yy-MM-dd HH:mm:ss.fff} Stopping");

            cancellationTokenSource.Cancel();

            Console.WriteLine($"{DateTime.UtcNow:yy-MM-dd HH:mm:ss.fff} Stopped");

            Console.WriteLine("Press ENTER to exit");
            Console.ReadLine();
         }
         catch (Exception ex)
         {
            Console.WriteLine($"{DateTime.UtcNow:yy-MM-dd HH:mm:ss} Application shutdown failure {ex.Message}", ex);
         }
      }

      private static async Task StartStreamProcessingAsync(InputSource inputSource, CancellationToken cancellationToken = default)
      {
         Console.WriteLine($"{DateTime.UtcNow:yy-MM-dd HH:mm:ss.fff} Started");

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

      private static void NewImageReceived(byte[] imageData)
      {
         Console.WriteLine($"{DateTime.UtcNow:yy-MM-dd HH:mm:ss.fff} New image received, bytes:{imageData.Length}");

         string outputPath = Path.Combine(_applicationSettings.SavePath, string.Format(_applicationSettings.FrameFileNameFormat, DateTime.UtcNow));

         File.WriteAllBytes(outputPath, imageData);
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
   }
}

