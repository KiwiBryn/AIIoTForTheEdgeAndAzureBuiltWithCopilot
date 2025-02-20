
using Microsoft.Extensions.Azure;

namespace ImageFileUploadHandler
{
   public class Program
   {
      public static void Main(string[] args)
      {
         var builder = WebApplication.CreateBuilder(args);

         // Add services to the container.
         builder.Services.AddAzureClients(azureClient =>
         {
            azureClient.AddQueueServiceClient(builder.Configuration.GetConnectionString("HandlerQueueStorage"));
            azureClient.AddBlobServiceClient(builder.Configuration.GetConnectionString("HandlerBlobStorage"));
         });

         builder.Services.AddControllers();
         // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
         builder.Services.AddEndpointsApiExplorer();
         builder.Services.AddSwaggerGen();

         var app = builder.Build();

         // Configure the HTTP request pipeline.
         if (app.Environment.IsDevelopment())
         {
            app.UseSwagger();
            app.UseSwaggerUI();
         }

         app.UseHttpsRedirection();

         app.UseAuthorization();


         app.MapControllers();

         app.Run();
      }
   }
}
