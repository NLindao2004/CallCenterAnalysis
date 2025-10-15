
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.ApplicationInsights; 
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CallAnalysis.Functions.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        // Registrar servicios personalizados
        services.AddSingleton<TextCleaningService>();

        services.AddSingleton(sp =>
        {
            var subscriptionKey = Environment.GetEnvironmentVariable("SpeechService_SubscriptionKey") ?? "";
            var region = Environment.GetEnvironmentVariable("SpeechService_Region") ?? "eastus";
            var logger = sp.GetRequiredService<ILogger<SpeechBatchService>>();
            return new SpeechBatchService(subscriptionKey, region, logger);
        });

        services.AddSingleton(sp =>
        {
            var endpoint = Environment.GetEnvironmentVariable("OpenAI_Endpoint") ?? "";
            var key = Environment.GetEnvironmentVariable("OpenAI_Key") ?? "";
            var deployment = Environment.GetEnvironmentVariable("OpenAI_DeploymentName") ?? "gpt-4";
            var logger = sp.GetRequiredService<ILogger<OpenAIAnalysisService>>();
            return new OpenAIAnalysisService(endpoint, key, deployment, logger);
        });

        services.AddSingleton(sp =>
        {
            var connectionString = Environment.GetEnvironmentVariable("DataLake_ConnectionString") ?? "";
            var logger = sp.GetRequiredService<ILogger<DataLakeService>>();
            return new DataLakeService(connectionString, logger);
        });
    })
    .ConfigureLogging(logging =>
    {
        logging.AddConsole();
        logging.SetMinimumLevel(LogLevel.Information);
    })
    .Build();

await host.RunAsync();