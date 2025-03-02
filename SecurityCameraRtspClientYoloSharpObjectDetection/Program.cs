//---------------------------------------------------------------------------------
// Copyright (c) March 2025, devMobile Software
//
// https://www.gnu.org/licenses/#AGPL
//
//---------------------------------------------------------------------------------
// Save image if object with specified name detected
// Modify so objectDetected supports multiple images names
// Modify objectNames to classNames
// Make confidence confgurable in app settings
// Make keep aspect ratio configurable in app settings
// Make SuppressParallelInference configurable in app settings
using Microsoft.Extensions.Configuration;

using Compunet.YoloSharp;

using RabbitOM.Streaming.Rtp.Framing;
using RabbitOM.Streaming.Rtp.Framing.Jpeg;
using RabbitOM.Streaming.Rtsp;
using RabbitOM.Streaming.Rtsp.Clients;


namespace SecurityCameraRtspClientYoloSharpObjectDetection
{
   class Program
   {
      private static ApplicationSettings _applicationSettings;
      private static YoloPredictor _yolo;

      private static readonly RtpFrameBuilder _frameBuilder = new JpegFrameBuilder();

      static void Main(string[] args)
      {
         Console.WriteLine($"{DateTime.UtcNow:yy-MM-dd HH:mm:ss} SecurityCameraRTSPClientNagerVideoStream");

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

         _yolo = new YoloPredictor(_applicationSettings.ModelPath, new YoloPredictorOptions()
         {
            Configuration = new YoloConfiguration()
            {
               Confidence = _applicationSettings.ConfidenceThreshold,
               KeepAspectRatio = _applicationSettings.KeepAspectRatio,
               SuppressParallelInference = _applicationSettings.SuppressParallelInference,
            }
         });

         _frameBuilder.FrameReceived += OnFrameReceived;

         using (var client = new RtspClient())
         {
            client.CommunicationStarted += (sender, e) =>
            {
               Console.ForegroundColor = ConsoleColor.White;
               Console.WriteLine("Communication started - " + DateTime.Now);
            };

            client.CommunicationStopped += (sender, e) =>
            {
               Console.ForegroundColor = ConsoleColor.White;
               Console.WriteLine("Communication stopped - " + DateTime.Now);
            };

            client.Connected += (sender, e) =>
            {
               Console.ForegroundColor = ConsoleColor.Yellow;
               Console.WriteLine("Client connected - " + client.Configuration.Uri);
            };

            client.Disconnected += (sender, e) =>
            {
               Console.ForegroundColor = ConsoleColor.Yellow;
               Console.WriteLine("Client disconnected - " + DateTime.Now);
            };

            client.Error += (sender, e) =>
            {
               Console.ForegroundColor = ConsoleColor.Red;
               Console.WriteLine("Client Error: " + (sender as RtspClient).Configuration.Uri + " " + e.Code);
            };

            client.PacketReceived += (sender, e) =>
            {
               var interleavedPacket = e.Packet as RtspInterleavedPacket;

               if (interleavedPacket != null && interleavedPacket.Channel > 0)
               {
                  // In most of case, avoid this packet
                  Console.ForegroundColor = ConsoleColor.DarkCyan;
                  Console.WriteLine("Skipping some data : size {0}", e.Packet.Data.Length);
                  return;
               }

               _frameBuilder.Write(interleavedPacket.Data); ;
            };

            client.Configuration.Uri = _applicationSettings.RtspCameraUrl;
            client.Configuration.UserName = _applicationSettings.UserName;
            client.Configuration.Password = _applicationSettings.Password;
            client.Configuration.ReceiveTimeout = TimeSpan.FromSeconds(3);
            client.Configuration.SendTimeout = TimeSpan.FromSeconds(3);
            client.Configuration.KeepAliveType = RtspKeepAliveType.Options;
            client.Configuration.MediaFormat = RtspMediaFormat.Video;
            client.Configuration.DeliveryMode = RtspDeliveryMode.Tcp;

            // client.Configuration.DeliveryMode = RtspDeliveryMode.Multicast;
            // client.Configuration.MulticastAddress = "229.0.0.1";
            // client.Configuration.RtpPort = 55000;
            // client.Configuration.TimeToLive = 15;

            client.StartCommunication();

            Console.CancelKeyPress += (sender, e) => Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine("Press any keys to close the application");
            Console.ReadKey();

            client.StopCommunication(TimeSpan.FromSeconds(3));

            Console.WriteLine("Press Enter to exit the application");
            Console.ReadLine();
         }
      }

      static DateTime LastTimeUtc = DateTime.UtcNow;

      private static void OnFrameReceived(object sender, RtpFrameReceivedEventArgs e)
      {
         //Console.WriteLine($"{DateTime.UtcNow:yy-MM-dd HH:mm:ss.fff} New image received, bytes:{e.Frame.Data.Length}");

         var start = DateTime.UtcNow;

         var detections = _yolo.Detect(e.Frame.Data);

         bool objectDetected = detections.Any(d => _applicationSettings.ClassNames.Contains(d.Name.Name));

         if (objectDetected)
         {
            string outputPath = Path.Combine(_applicationSettings.SavePath, string.Format(_applicationSettings.FrameFileNameFormat, DateTime.UtcNow));
            File.WriteAllBytes(outputPath, e.Frame.Data);

            foreach (var detection in detections)
            {
               Console.WriteLine($" Name: {detection.Name.Name} Confidence:{detection.Confidence} Bounding Box{detection.Bounds}");
            }
         }

         Console.WriteLine($"{DateTime.UtcNow:yy-MM-dd HH:mm:ss.fff} LastTime:{(DateTime.UtcNow - LastTimeUtc).TotalMilliseconds:0} mSec Duration:{(DateTime.UtcNow - start).TotalMilliseconds:0} mSec");
         LastTimeUtc = DateTime.UtcNow;
      }
   }

   public class ApplicationSettings
   {
      public string RtspCameraUrl { get; set; }

      public string UserName { get; set; }

      public string Password { get; set; }

      public string SavePath { get; set; }

      public string FrameFileNameFormat { get; set; } 

      public string ModelPath { get; set; }

      public List<string> ClassNames { get; set; } = new List<string>();

      public float ConfidenceThreshold { get; set; } = 0.75f;

      public bool KeepAspectRatio { get; set; } = true;

      public bool SuppressParallelInference { get; set; } = true;
   }
}
