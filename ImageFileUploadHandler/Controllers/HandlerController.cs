// please write a controller that receives an uploaded image and inserts it into a azure storage queue
// Modify the code to use the claim check pattern
// DeviceID on the requesturi, add as metadata of the blob 

using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Microsoft.AspNetCore.Mvc;

namespace ImageFileUploadHandler.Controllers
{
   [Route("api/[controller]")]
   [ApiController]
   public class HandlerController : ControllerBase
   {
      private readonly string _queueName = "imagestobeprocessed";
      //private readonly string _blobContainerName = "your-blob-container-name";
      QueueServiceClient queueServiceClient;
      BlobServiceClient blobServiceClient;

      public HandlerController([FromServices] QueueServiceClient queueServiceClient, [FromServices] BlobServiceClient blobServiceClient)
      {
         this.queueServiceClient = queueServiceClient;
         this.blobServiceClient = blobServiceClient;
      }

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
            //BlobContainerClient blobContainerClient = new BlobContainerClient(_storageConnectionString, _blobContainerName);
            BlobContainerClient blobContainerClient =  blobServiceClient.GetBlobContainerClient(deviceId.ToLower());
            await blobContainerClient.CreateIfNotExistsAsync();
            //string blobName = Guid.NewGuid().ToString();
            //BlobClient blobClient = blobContainerClient.GetBlobClient(blobName);
            BlobClient blobClient = blobContainerClient.GetBlobClient(image.FileName);


            var metadata = new Dictionary<string, string>
            {
               { "DeviceID", deviceId }
            };

            using (var memoryStream = new MemoryStream())
            {
               await image.CopyToAsync(memoryStream);
               memoryStream.Position = 0;
               await blobClient.UploadAsync(memoryStream, true);
               await blobClient.SetMetadataAsync(metadata);
            }

            // Insert blob URL into Azure Storage Queue
            //string blobUrl = blobClient.Uri.ToString();
            //string blobUrl = blobClient.Uri.ToString();
            //QueueClient queueClient = new QueueClient(_storageConnectionString, _queueName);
            QueueClient queueClient = queueServiceClient.GetQueueClient(_queueName);
            await queueClient.CreateIfNotExistsAsync();
            if (queueClient.Exists())
            {
               await queueClient.SendMessageAsync(image.FileName);
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
