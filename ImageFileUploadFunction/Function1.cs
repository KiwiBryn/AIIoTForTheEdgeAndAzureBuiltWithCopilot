// the payload is  multi-part form data 
//using System;
//using System.IO;
//using System.Threading.Tasks;
using System.Text;
using System.Text.Json;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.Functions.Worker;
//using Microsoft.Azure.Functions.Worker.Http;
//using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;


public class function1
{
   private readonly ILogger _logger;

   public function1(ILoggerFactory loggerFactory)
   {
      _logger = loggerFactory.CreateLogger<function1>();
   }

   [Function("UploadImageFunction")]
   public async Task<IActionResult> Run(
       //[HttpTrigger(AuthorizationLevel.Function, "post", Route = "UploadImage")] HttpRequestData req)
       [HttpTrigger(AuthorizationLevel.Function, "post", Route = "UploadImage")] HttpRequest req)
   {
      _logger.LogInformation("UploadImageFunction processed a request.");

      // Parse the query string to retrieve the DeviceID.
      //var queryDictionary = QueryHelpers.ParseQuery(req.Query.ToString());
      //string deviceId = queryDictionary.ContainsKey("DeviceID") ? queryDictionary["DeviceID"].ToString() : null;
      string deviceId = req.Query.ContainsKey("DeviceID") ? req.Query["DeviceID"].ToString() : null;
      if (string.IsNullOrWhiteSpace(deviceId))
      {
         //var badReq = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
         //await badReq.WriteStringAsync("Please pass a DeviceID in the query string.");
         //return badReq;
         return new BadRequestObjectResult("Please upload exactly one image file.");
      }

      // Parse the multi-part form data
      var formCollection = await req.ReadFormAsync();
      if (!formCollection.Files.Any())
      {
         //var badReq = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
         //await badReq.WriteStringAsync("No file found in the request.");
         //return badReq;
         return new BadRequestObjectResult("Please upload exactly one image file.");
      }

      IFormFile file = formCollection.Files.First();
      byte[] imageBytes;
      using (var memoryStream = new MemoryStream())
      {
         await file.CopyToAsync(memoryStream);
         imageBytes = memoryStream.ToArray();
      }

      // Write the decoded image bytes to the output blob (the container is specified via DeviceID).
      using (var imageBlob = await GetBlobStreamAsync(deviceId.ToLower(), file.FileName))
      {
         await imageBlob.WriteAsync(imageBytes, 0, imageBytes.Length);
      }

      // Create a JSON message for the queue with metadata.
      var queueMessage = new
      {
         DeviceID = deviceId,
         BlobName = file.FileName,
         ImageCreatedAtUtc = req.Headers["ImageCreatedAtUtc"].ToString()
      };

      string messagePayload = JsonSerializer.Serialize(queueMessage);
      await AddToQueueAsync(messagePayload);

      //var okResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);
      //await okResponse.WriteStringAsync("Image uploaded successfully.");
      //return okResponse;
      return new OkObjectResult("Image uploaded successfully.");
   }

   // Helper method to get a writable blob stream.
   private async Task<Stream> GetBlobStreamAsync(string deviceId, string filename)
   {
      var blobServiceClient = new Azure.Storage.Blobs.BlobServiceClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
      var containerClient = blobServiceClient.GetBlobContainerClient(deviceId);
      await containerClient.CreateIfNotExistsAsync();
      //var blobClient = containerClient.GetBlobClient(Guid.NewGuid().ToString() + ".jpg");
      var blobClient = containerClient.GetBlobClient(filename);
      return await blobClient.OpenWriteAsync(true);
   }

   // Helper method to add a message to the queue.
   private async Task AddToQueueAsync(string messagePayload)
   {
      var queueClient = new Azure.Storage.Queues.QueueClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"), "imagestobeprocessed");
      await queueClient.CreateIfNotExistsAsync();
      await queueClient.SendMessageAsync(Convert.ToBase64String(Encoding.UTF8.GetBytes(messagePayload)));
   }
}
