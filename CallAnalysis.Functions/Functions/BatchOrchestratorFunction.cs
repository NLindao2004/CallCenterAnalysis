using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CallAnalysis.Functions.Models;

namespace CallAnalysis.Functions.Functions
{
    /// <summary>
    /// Orquestador principal del proceso batch nocturno D-1
    /// Trigger: 00:30 AM (como especifica la arquitectura)
    /// </summary>
    public class BatchOrchestratorFunction
    {
        private readonly ILogger<BatchOrchestratorFunction> _logger;

        public BatchOrchestratorFunction(ILogger<BatchOrchestratorFunction> logger)
        {
            _logger = logger;
        }

        [Function("BatchOrchestrator_Timer")]
        public async Task RunTimer(
            [TimerTrigger("0 30 0 * * *")] TimerInfo timerInfo,
            [DurableClient] DurableTaskClient client,
            FunctionContext context)
        {
            var logger = context.GetLogger("BatchOrchestrator_Timer");
            logger.LogInformation($"⏰ Iniciando proceso batch D-1: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");

            var processDate = DateTime.UtcNow.AddDays(-1).Date;

            var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
                nameof(ProcessDailyBatch),
                new { ProcessDate = processDate.ToString("yyyy-MM-dd") });

            logger.LogInformation($"✓ Orquestación iniciada. InstanceId: {instanceId}");
        }

        [Function(nameof(ProcessDailyBatch))]
        public async Task<string> ProcessDailyBatch(
            [OrchestrationTrigger] TaskOrchestrationContext context)
        {
            var input = context.GetInput<dynamic>();
            var processDateStr = (string)input!.ProcessDate;
            var processDate = DateTime.Parse(processDateStr);

            var logger = context.CreateReplaySafeLogger<BatchOrchestratorFunction>();
            logger.LogInformation($"📊 Procesando batch para: {processDate:yyyy-MM-dd}");

            try
            {
                // PASO 1: Obtener lista de grabaciones del día D-1
                var recordings = await context.CallActivityAsync<List<CallRecording>>(
                    nameof(GetDailyRecordingsActivity),
                    processDateStr);

                logger.LogInformation($"📁 Encontradas {recordings.Count} grabaciones");

                if (!recordings.Any())
                {
                    logger.LogWarning("⚠️ No hay grabaciones para procesar");
                    return "Sin grabaciones para procesar";
                }

                // PASO 2: Procesar en lotes de 50 (según límites de Azure)
                var batchSize = 50;
                var processedCount = 0;
                var allAnalysisResults = new List<AnalysisResult>();

                for (int i = 0; i < recordings.Count; i += batchSize)
                {
                    var batch = recordings.Skip(i).Take(batchSize).ToList();
                    logger.LogInformation($"🔄 Procesando lote {(i / batchSize) + 1} ({batch.Count} llamadas)");

                    // Transcripción + Limpieza + Análisis en paralelo
                    var batchTasks = batch.Select(async recording =>
                    {
                        // Transcribir
                        var transcription = await context.CallActivityAsync<TranscriptionResult>(
                            nameof(TranscribeAndCleanActivity),
                            recording);

                        // Analizar con OpenAI
                        var analysis = await context.CallActivityAsync<AnalysisResult>(
                            nameof(AnalyzeWithOpenAIActivity),
                            transcription);

                        return analysis;
                    });

                    var batchResults = await Task.WhenAll(batchTasks);
                    allAnalysisResults.AddRange(batchResults);
                    processedCount += batch.Count;

                    logger.LogInformation($"✓ Lote completado. Total procesado: {processedCount}/{recordings.Count}");
                }

                // PASO 3: Escribir a Data Lake GOLD
                await context.CallActivityAsync(
                    nameof(WriteToGoldLayerActivity),
                    new { Results = allAnalysisResults, PartitionDate = processDateStr });

                // PASO 4: Upsert a SQL Server local (para Power BI)
                await context.CallActivityAsync(
                    nameof(UpsertToSQLActivity),
                    allAnalysisResults);

                var summary = $"✅ Proceso completado: {processedCount} llamadas analizadas para {processDate:yyyy-MM-dd}";
                logger.LogInformation(summary);

                return summary;
            }
            catch (Exception ex)
            {
                logger.LogError($"❌ Error en orquestación: {ex.Message}");
                throw;
            }
        }

        [Function(nameof(GetDailyRecordingsActivity))]
        public List<CallRecording> GetDailyRecordingsActivity(
            [ActivityTrigger] string processDate,
            FunctionContext context)
        {
            var logger = context.GetLogger(nameof(GetDailyRecordingsActivity));
            logger.LogInformation($"🔍 Buscando grabaciones para: {processDate}");

            // TODO: Implementar consulta a Blob Storage
            // Los archivos están en el container "raw" copiados por AzCopy
            // Filtrar por fecha del día anterior (D-1)

            return new List<CallRecording>
            {
                new CallRecording
                {
                    CallId = $"CALL-{DateTime.Parse(processDate):yyyyMMdd}-001",
                    BlobUri = "https://stcallanalysis.blob.core.windows.net/raw/...",
                    CallDate = DateTime.Parse(processDate),
                    AgentId = "AGT001",
                    Status = "Pending"
                }
            };
        }

        [Function(nameof(TranscribeAndCleanActivity))]
        public TranscriptionResult TranscribeAndCleanActivity(
            [ActivityTrigger] CallRecording recording,
            FunctionContext context)
        {
            var logger = context.GetLogger(nameof(TranscribeAndCleanActivity));
            logger.LogInformation($"🎤 Transcribiendo y limpiando: {recording.CallId}");

            // TODO: Implementar integración con Speech Batch API
            // La transcripción ya debe estar lista desde el proceso de ADF

            return new TranscriptionResult
            {
                CallId = recording.CallId,
                RawText = "Transcripción simulada...",
                TranscriptionTimestamp = DateTime.UtcNow,
                ConfidenceScore = 0.95
            };
        }

        [Function(nameof(AnalyzeWithOpenAIActivity))]
        public AnalysisResult AnalyzeWithOpenAIActivity(
            [ActivityTrigger] TranscriptionResult transcription,
            FunctionContext context)
        {
            var logger = context.GetLogger(nameof(AnalyzeWithOpenAIActivity));
            logger.LogInformation($"🤖 Analizando con OpenAI: {transcription.CallId}");

            // TODO: Implementar llamada a OpenAIAnalysisService

            return new AnalysisResult
            {
                CallId = transcription.CallId,
                AnalysisDate = DateTime.UtcNow
            };
        }

        [Function(nameof(WriteToGoldLayerActivity))]
        public async Task WriteToGoldLayerActivity(
            [ActivityTrigger] dynamic input,
            FunctionContext context)
        {
            var logger = context.GetLogger(nameof(WriteToGoldLayerActivity));
            logger.LogInformation("💾 Escribiendo a capa Oro (Data Lake)");

            // TODO: Implementar escritura a Data Lake usando DataLakeService

            await Task.CompletedTask;
        }

        [Function(nameof(UpsertToSQLActivity))]
        public async Task UpsertToSQLActivity(
            [ActivityTrigger] List<AnalysisResult> results,
            FunctionContext context)
        {
            var logger = context.GetLogger(nameof(UpsertToSQLActivity));
            logger.LogInformation($"💽 Haciendo upsert de {results.Count} registros a SQL Server");

            // TODO: Implementar upsert a SQL Server local

            await Task.CompletedTask;
        }
    }
}