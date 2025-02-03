// Start withj SecurityCameraHttpClient code
using System.Net;

using Microsoft.Extensions.Configuration;


namespace SecurityCameraHttpClientYoloSharpObjectDetection
{
   internal class Program
   {
      private static HttpClient _client;
      private static bool _isRetrievingImage = false;
      private static ApplicationSettings _applicationSettings;

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

            byte[] imageBytes = await response.Content.ReadAsByteArrayAsync();
            string savePath = string.Format(_applicationSettings.SavePath, DateTime.UtcNow);
            await File.WriteAllBytesAsync(savePath, imageBytes);

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
   }
}
