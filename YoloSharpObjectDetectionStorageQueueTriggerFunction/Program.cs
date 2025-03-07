using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Application Insights isn't enabled by default. See https://aka.ms/AAt8mw4.
/*
builder.Services
    .AddApplicationInsightsTelemetryWorkerService(builder.Configuration.GetSection("ApplicationInsights"))
    .ConfigureFunctionsApplicationInsights()
    .Configure<LoggerFilterOptions>(options =>
    {
       // The Application Insights SDK adds a default logging filter that instructs ILogger to capture only Warning and more severe logs. Application Insights requires an explicit override.
       // Log levels can also be configured using appsettings.json. For more information, see https://learn.microsoft.com/en-us/azure/azure-monitor/app/worker-service#ilogger-logs
       LoggerFilterRule toRemove = options.Rules.FirstOrDefault(rule => rule.ProviderName == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");
       if (toRemove is not null)
       {
          options.Rules.Remove(toRemove);
       }
    });
*/
builder.Configuration.AddUserSecrets<Program>();
builder.Services.AddAzureClients(azureClient =>
{
   azureClient.AddBlobServiceClient(builder.Configuration.GetConnectionString("ImageProcessorBlobStorage"));
});


builder.Build().Run();
