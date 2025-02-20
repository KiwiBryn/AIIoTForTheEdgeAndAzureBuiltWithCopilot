// please write a console application that uses httpclient to upload a file to a webapi
// modify to upload all the files in a directory
namespace ImageFileUploader
{
   internal class Program
   {
      static async Task Main(string[] args)
      {
         if (args.Length < 2)
         {
            Console.WriteLine("Usage: ImageFileUploader <directory_path> <api_url>");
            return;
         }

         string directoryPath = args[0];
         string apiUrl = args[1];

         if (!Directory.Exists(directoryPath))
         {
            Console.WriteLine("Directory not found: " + directoryPath);
            return;
         }

         try
         {
            using (HttpClient client = new HttpClient())
            {
               foreach (string filePath in Directory.GetFiles(directoryPath))
               {
                  using (MultipartFormDataContent content = new MultipartFormDataContent())
                  {
                     byte[] fileBytes = await File.ReadAllBytesAsync(filePath);
                     ByteArrayContent byteContent = new ByteArrayContent(fileBytes);
                     content.Add(byteContent, "file", Path.GetFileName(filePath));

                     HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                     if (response.IsSuccessStatusCode)
                     {
                        Console.WriteLine($"File {Path.GetFileName(filePath)} uploaded successfully.");
                     }
                     else
                     {
                        Console.WriteLine($"File {Path.GetFileName(filePath)} upload failed. Status code: " + response.StatusCode);
                     }
                  }
               }
            }
         }
         catch (Exception ex)
         {
            Console.WriteLine("An error occurred: " + ex.Message);
         }
      }
   }
}
