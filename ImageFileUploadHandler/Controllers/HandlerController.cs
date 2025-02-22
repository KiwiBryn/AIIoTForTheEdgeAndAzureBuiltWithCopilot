// please write a controller that receives an uploaded image and inserts it into a azure storage queue
// Modify the code to use the claim check pattern
// DeviceID on the requesturi, add as metadata of the blob 
// Mobile HandlerController to use primary constructor

using System.Text.Json;

using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Microsoft.AspNetCore.Mvc;

namespace ImageFileUploadHandler.Controllers
{
   [Route("api/[controller]")]
   [ApiController]
   public class HandlerController(QueueServiceClient queueServiceClient, BlobServiceClient blobServiceClient) : ControllerBase
   {
      private readonly string _queueName = "imagestobeprocessed";

      [HttpPost("upload/{deviceId}")]
      public async Task<IActionResult> UploadImage(string deviceId, IFormFile image)
      {
         if (image == null || image.Length == 0)
         {
            return BadRequest("No image uploaded.");
         }

         try
         {
            // Upload image to Azure Blob Storage
            BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient(deviceId.ToLower());
            await blobContainerClient.CreateIfNotExistsAsync();
            BlobClient blobClient = blobContainerClient.GetBlobClient(image.FileName);

            var metadata = new Dictionary<string, string>
            {
               { "DeviceID", deviceId },
               { "File-Creation-Time", DateTime.UtcNow.ToString("o") }
            };

            using (var memoryStream = new MemoryStream())
            {
               await image.CopyToAsync(memoryStream);
               memoryStream.Position = 0;
               await blobClient.UploadAsync(memoryStream, true);
               await blobClient.SetMetadataAsync(metadata);
            }

            // Insert blob URL into Azure Storage Queue
            QueueClient queueClient = queueServiceClient.GetQueueClient(_queueName);
            await queueClient.CreateIfNotExistsAsync();

            // Create payload for the queue
            var payload = new
            {
               DeviceID = deviceId,
               BlobName = image.FileName
            };

            // Insert payload into Azure Storage Queue
            if (queueClient.Exists())
            {
               await queueClient.SendMessageAsync(Convert.ToBase64String(JsonSerializer.SerializeToUtf8Bytes(payload)));
            }

            return Ok("Image uploaded and inserted into queue successfully.");
         }
         catch (Exception ex)
         {
            return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
         }
      }
   }
}
