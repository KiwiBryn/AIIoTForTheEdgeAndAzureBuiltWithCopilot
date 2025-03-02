// please write a console application that uses httpclient to upload a file to a webapi
// modify to upload all the files in a directory
// use a System.Threading.Timer to upload the files every 1000mSec
// retrieve the directoryPath, apiUrl, duetime and period from an appsettings.json file 
// make the settings strongly typed
// Delete the file once it has been uploaded
// Add code to stop UploadFiles being called when an upload is in progress
// move LoadConfiguration into main
// make the deletion of files configurable
// move uploadTimer to main 
// Add a configurable DeviceID to post requesturi
// Add the creation time of the image file as a header of the request
using Microsoft.Extensions.Configuration;

namespace ImageFileUploader
{
   internal class Program
   {
      private static ApplicationSettings _applicationSettings;
      private static bool isUploading = false;

      static async Task Main(string[] args)
      {
         Console.WriteLine($"{DateTime.UtcNow:yy-MM-dd HH:mm:ss} ImageFileUploader");

         var configuration = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile("appsettings.json")
             .AddUserSecrets<Program>()
             .Build();

         _applicationSettings = configuration.GetSection("ApplicationSettings").Get<ApplicationSettings>();

         if (!Directory.Exists(_applicationSettings.DirectoryPath))
         {
            Console.WriteLine("Directory not found: " + _applicationSettings.DirectoryPath);
            return;
         }

         Timer uploadTimer = new Timer(async _ => await UploadFiles(), null, _applicationSettings.DueTime, _applicationSettings.Period);

         Console.WriteLine("Press [Enter] to exit the program.");
         Console.ReadLine();
      }

      private static async Task UploadFiles()
      {
         if (isUploading) return;

         isUploading = true;

         try
         {
            using (HttpClient client = new HttpClient())
            {
               foreach (string filePath in Directory.GetFiles(_applicationSettings.DirectoryPath))
               {
                  using (MultipartFormDataContent content = new MultipartFormDataContent())
                  {
                     byte[] fileBytes = await File.ReadAllBytesAsync(filePath);
                     ByteArrayContent byteContent = new ByteArrayContent(fileBytes);
                     content.Add(byteContent, "image", Path.GetFileName(filePath));

                     // Add file creation time as a header
                     DateTime creationTime = File.GetCreationTime(filePath);
                     client.DefaultRequestHeaders.Add("ImageCreatedAtUtc", creationTime.ToString("o"));

                     string requestUri = $"{_applicationSettings.ApiUrl}/{_applicationSettings.DeviceID}";
                     HttpResponseMessage response = await client.PostAsync(requestUri, content);
                     if (response.IsSuccessStatusCode)
                     {
                        Console.WriteLine($"File {Path.GetFileName(filePath)} uploaded successfully.");
                     }
                     else
                     {
                        Console.WriteLine($"File {Path.GetFileName(filePath)} upload failed. Status code: " + response.StatusCode);
                     }
                     client.DefaultRequestHeaders.Remove("ImageCreatedAtUtc");
                  }

                  if (_applicationSettings.DeleteAfterUpload)
                  {
                     File.Delete(filePath);
                  }
               }
            }
         }
         catch (Exception ex)
         {
            Console.WriteLine("An error occurred: " + ex.Message);
         }
         finally
         {
            isUploading = false;
         }
      }
   }

   public class ApplicationSettings
   {
      public string DirectoryPath { get; set; }
      public string ApiUrl { get; set; }
      public int DueTime { get; set; }
      public int Period { get; set; }
      public bool DeleteAfterUpload { get; set; }
      public string DeviceID { get; set; }
   }
}
