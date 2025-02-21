using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Application Insights isn't enabled by default. See https://aka.ms/AAt8mw4.
// builder.Services
//     .AddApplicationInsightsTelemetryWorkerService()
//     .ConfigureFunctionsApplicationInsights();

builder.Configuration.AddUserSecrets<Program>();
builder.Services.AddAzureClients(azureClient =>
{
   azureClient.AddBlobServiceClient(builder.Configuration.GetConnectionString("ImageProcessorBlobStorage"));
});


builder.Build().Run();
