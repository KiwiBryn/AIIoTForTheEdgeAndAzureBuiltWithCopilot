// please modify the code to use compunet yolosharp to run an Ultralytics yolo object detection model on an image loaded Azure Storage
// Modify the code the image name and deviceID which is the container name are JSON serialized in the message
// Modify the code so BlobClient is a injected dependency
// Modify the code to use an Output binding to write the result to a Azure Storage Queue called imageinferencingresults

using System.Text.Json;

using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

using Compunet.YoloSharp;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace YoloSharpObjectDetectionStorageQueueTriggerFunction
{
   public class Function1
   {
      private readonly ILogger<Function1> _logger;
      private readonly YoloPredictor _yolo;
      private readonly BlobServiceClient _blobServiceClient;

      public Function1(ILogger<Function1> logger, BlobServiceClient blobServiceClient)
      {
         _logger = logger;
         _yolo = new YoloPredictor("yolov8s.onnx"); // Load your YOLO model here
         //_yolo = new YoloPredictor("PPEV220250226V1");
         _blobServiceClient = blobServiceClient;
      }

      [Function(nameof(Function1))]
      [QueueOutput("imageinferencingresults", Connection = "ImageProcessorQueueStorage")]
      public async Task<string> Run([QueueTrigger("imagestobeprocessed", Connection = "ImageProcessorQueueStorage")] QueueMessage message)
      {
         _logger.LogInformation("C# Queue trigger function processed: {MessageText}", message.MessageText);

         var messageData = JsonSerializer.Deserialize<MessageData>(message.MessageText);

         BlobClient blobClient = _blobServiceClient.GetBlobContainerClient(messageData.DeviceID.ToLower()).GetBlobClient(messageData.BlobName);
         BlobDownloadInfo download = await blobClient.DownloadAsync();

         using (MemoryStream ms = new MemoryStream())
         {
            await download.Content.CopyToAsync(ms);
            ms.Position = 0;

            var image = Image.Load<Rgb24>(ms);
            var results = await _yolo.DetectAsync(image);

            foreach (var result in results)
            {
                _logger.LogInformation("Detected object: {ObjectName} with confidence {Confidence}", result.Name.Name, result.Confidence);
            }

            return JsonSerializer.Serialize(new { messageData.DeviceID, messageData.BlobName, messageData.ImageCreatedAtUtc, Detections = results });
         }
      }
   }

   internal class MessageData
   {
      public string DeviceID { get; set; }
      public string BlobName { get; set; }
      public string ImageCreatedAtUtc { get; set; }

   }
}
