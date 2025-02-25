using Microsoft.Extensions.Configuration;

using RabbitOM.Streaming.Rtp.Framing;
using RabbitOM.Streaming.Rtp.Framing.Jpeg;
using RabbitOM.Streaming.Rtsp;
using RabbitOM.Streaming.Rtsp.Clients;


namespace SecurityCameraRTSPClientRabbitOM
{
   class Program
   {
      private static ApplicationSettings _applicationSettings;

      private static readonly RtpFrameBuilder _frameBuilder = new JpegFrameBuilder();

      static void Main(string[] args)
      {
         Console.WriteLine($"{DateTime.UtcNow:yy-MM-dd HH:mm:ss} SecurityCameraRTSPClientRabbitOM");
#if RELEASE
         Console.WriteLine("RELEASE");
#else
         Console.WriteLine("DEBUG");
#endif

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

            Console.WriteLine($"{DateTime.UtcNow:yy-MM-dd HH:mm:ss.fff} Starting");
            client.StartCommunication();
            Console.WriteLine($"{DateTime.UtcNow:yy-MM-dd HH:mm:ss.fff} Started");


            Console.CancelKeyPress += (sender, e) => Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine($"{DateTime.UtcNow:yy-MM-dd HH:mm:ss.fff} Stopping");

            Console.WriteLine("Press any keys to close the application");
            Console.ReadKey();

            client.StopCommunication(TimeSpan.FromSeconds(3));

            Console.WriteLine("Press ENTER to exit");
            Console.ReadLine();
         }
      }

      private static void OnFrameReceived(object sender, RtpFrameReceivedEventArgs e)
      {
         Console.WriteLine($"{DateTime.UtcNow:yy-MM-dd HH:mm:ss.fff} New image received, bytes:{e.Frame.Data.Length}");

         File.WriteAllBytes(Path.Combine(_applicationSettings.SavePath, string.Format(_applicationSettings.FrameFileNameFormat, DateTime.UtcNow)), e.Frame.Data);
      }
   }

   public class ApplicationSettings
   {
      public string RtspCameraUrl { get; set; }

      public string UserName { get; set; }

      public string Password { get; set; }

      public string SavePath { get; set; } = "";

      public string FrameFileNameFormat { get; set; } = "";
   }
}
