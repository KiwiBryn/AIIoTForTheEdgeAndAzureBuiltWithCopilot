using System.Drawing;

using Microsoft.Extensions.Configuration;

using FFMpegCore.Pipes;
using FFMpegCore;
using FFMpegCore.Enums;

using SkiaSharp;


namespace SecurityCameraRTSPClientFFMpegCore
{
   class Program
   {
      static async Task Main(string[] args)
      {
         ApplicationSettings _applicationSettings;

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

         if ( Directory.Exists(_applicationSettings.SavePath) == false)
         {
            Directory.CreateDirectory(_applicationSettings.SavePath);
         }

         GlobalFFOptions.Configure(options => options.BinaryFolder = _applicationSettings.FFMpegExeFolder);

         var cts = new CancellationTokenSource();

         using (var ms = new MemoryStream())
         {
            await FFMpegArguments
                .FromUrlInput(new Uri(_applicationSettings.CameraUrl))
                .OutputToPipe(new StreamPipeSink(ms), options => options
                   .ForceFormat("mpeg1video")
                  //.ForceFormat("rawvideo")
                  .WithCustomArgument("-rtsp_transport tcp")
                    .WithFramerate(10)
                    .WithVideoCodec(VideoCodec.Png)
                    //.Resize(1024, 1024)
                    //.ForceFormat("image2pipe")
                    //.Resize(new Size(Config.JpgWidthLarge, Config.JpgHeightLarge))
                    //.Resize(new Size(Config.JpgWidthLarge, Config.JpgHeightLarge))
                    //.WithCustomArgument("-vf fps=1 -update 1")
                    //.WithCustomArgument("-vf fps=5 -update 1")
                )
                .NotifyOnProgress(o =>
                {
                   try
                   {
                      if (ms.Length > 0)
                      {
                         ms.Position = 0;

                         string outputPath = Path.Combine(_applicationSettings.SavePath, string.Format(_applicationSettings.FrameFileNameFormat, DateTime.UtcNow ));

                         using (var bitmap = new Bitmap(ms))
                         {
                            // Save the bitmap
                            bitmap.Save(outputPath);
                         }
                         ms.Position = 0;

                         /*
                         using (var skBitmap = SKBitmap.Decode(ms))
                         {
                            if (skBitmap is not null)
                            {
                               using (var skImage = SKImage.FromBitmap(skBitmap))
                               using (var data = skImage.Encode(SKEncodedImageFormat.Png, 100))
                               using (var stream = File.OpenWrite(outputPath))
                               {
                                  data.SaveTo(stream);
                               }
                            }
                         }
                         */
                         ms.SetLength(0);
                      }
                   }
                   catch (Exception ex)
                   {
                      Console.WriteLine(ex.Message);
                   }
                })
                .ProcessAsynchronously();
         }


         Console.WriteLine("Press any key to stop capturing...");
         Console.ReadKey();
         cts.Cancel();

         Console.WriteLine("Capture stopped.");
      }


      public class ApplicationSettings
      {
         public string CameraUrl { get; set; } = "";

         public string SavePath { get; set; } = "";

         public string FrameFileNameFormat { get; set; } = "";

         public string FFMpegExeFolder { get; set; } = "";
      }
   }
}