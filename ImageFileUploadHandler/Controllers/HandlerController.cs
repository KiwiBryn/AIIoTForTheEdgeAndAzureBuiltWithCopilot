// please write a controller that receives an uploaded image and inserts it into a azure storage queue
using Azure.Storage.Queues;
using Microsoft.AspNetCore.Mvc;

namespace ImageFileUploadHandler.Controllers
{
   [Route("api/[controller]")]
   [ApiController]
   public class HandlerController : ControllerBase
   {
      private readonly string _storageConnectionString = "YourAzureStorageConnectionString";
      private readonly string _queueName = "your-queue-name";

      [HttpPost("upload")]
      public async Task<IActionResult> UploadImage(IFormFile image)
      {
         if (image == null || image.Length == 0)
         {
            return BadRequest("No image uploaded.");
         }

         try
         {
            // Convert image to base64 string
            using (var memoryStream = new MemoryStream())
            {
               await image.CopyToAsync(memoryStream);
               var imageBytes = memoryStream.ToArray();
               var base64Image = Convert.ToBase64String(imageBytes);

               // Insert base64 string into Azure Storage Queue
               QueueClient queueClient = new QueueClient(_storageConnectionString, _queueName);
               await queueClient.CreateIfNotExistsAsync();
               if (queueClient.Exists())
               {
                  await queueClient.SendMessageAsync(base64Image);
               }
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
