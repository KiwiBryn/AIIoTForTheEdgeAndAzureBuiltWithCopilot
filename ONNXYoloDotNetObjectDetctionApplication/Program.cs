
/*
// please write a C# console application that uses YoloDotNet by Niklas Swärd to run an Ultralytics yolo object detection model on an image loaded from disk
namespace ONNXYoloDotNetObjectDetctionApplication
{
   internal class Program
   {
      static void Main(string[] args)
      {
         if (args.Length < 3)
         {
            Console.WriteLine("Usage: <executable> <model config path> <weights path> <image path>");
            return;
         }

         string modelConfigPath = args[0];
         string weightsPath = args[1];
         string imagePath = args[2];

         if (!File.Exists(modelConfigPath) || !File.Exists(weightsPath) || !File.Exists(imagePath))
         {
            Console.WriteLine("One or more files do not exist.");
            return;
         }

         using (var yoloWrapper = new YoloWrapper(modelConfigPath, weightsPath))
         {
            using (var image = Image.FromFile(imagePath))
            {
               var memoryStream = new MemoryStream();
               image.Save(memoryStream, image.RawFormat);
               var imageBytes = memoryStream.ToArray();

               var items = yoloWrapper.Detect(imageBytes);

               foreach (var item in items)
               {
                  Console.WriteLine($"Object: {item.Type}, Confidence: {item.Confidence}, X: {item.X}, Y: {item.Y}, Width: {item.Width}, Height: {item.Height}");
               }
            }
         }
      }
   }
}
*/