using System.Collections.Concurrent;

using Azure.Monitor.OpenTelemetry.AspNetCore;

using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.ML.OnnxRuntime;


var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Configuration.AddUserSecrets<Program>()
   .AddJsonFile("app.settings.json", optional: true);

builder.Services.AddOpenTelemetry().UseAzureMonitor();

builder.Logging.Services.Configure<LoggerFilterOptions>(options =>
{
   LoggerFilterRule? defaultRule = options.Rules.FirstOrDefault(rule => rule.ProviderName == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");
   if (defaultRule is not null)
   {
      options.Rules.Remove(defaultRule);
   }
});

//builder.Services.AddSingleton<IOnnxPredictionPool>(provider => new OnnxPredictionEnginePool(Path.Combine(AppContext.BaseDirectory, builder.Configuration.GetValue<string>("OnnxModelPath")), builder.Configuration.GetValue<int>("OnnxInferenceSessionPoolSize")));

builder.Build().Run();


public interface IOnnxPredictionPool
{
   public IDisposableReadOnlyCollection<DisposableNamedOnnxValue> Run(List<NamedOnnxValue> inputs);

   public InferenceSession Get();

   public void Return(InferenceSession session);
}

public class OnnxPredictionEnginePool : IOnnxPredictionPool
{
   private readonly ConcurrentBag<InferenceSession> _sessions = [];
   private readonly string _modelPath;

   public OnnxPredictionEnginePool(string modelPath, int poolSize = 5)
   {
      _modelPath = modelPath;

      _sessions = new ConcurrentBag<InferenceSession>();
      for (int i = 0; i < poolSize; i++)
      {
         //_sessions.Add(new InferenceSession(Path.Combine(AppContext.BaseDirectory, "optimized.onnx"), new SessionOptions()
         _sessions.Add(new InferenceSession(Path.Combine(AppContext.BaseDirectory, "FasterRCNN-10.onnx"), new SessionOptions()
         {
            ExecutionMode = ExecutionMode.ORT_PARALLEL,
         }));
      }
   }

   // Get a session from the pool
   public InferenceSession Get()
   {
      if (!_sessions.TryTake(out var session))
      {
         session = new InferenceSession(_modelPath);
      }

      return session;
   }

   // Return the session back to the pool
   public void Return(InferenceSession session)
   {
      _sessions.Add(session);
   }

   public IDisposableReadOnlyCollection<DisposableNamedOnnxValue> Run(List<NamedOnnxValue> inputs)
   {
      var session = Get();

      try
      {
         return session.Run(inputs);
      }
      finally
      {
         Return(session);
      }
   }
}
