using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using CallAnalysis.Functions.Models;

namespace CallAnalysis.Functions.Services
{
    /// <summary>
    /// Servicio para análisis con Azure OpenAI GPT-4
    /// Extrae: motivo de venta perdida, citas, clasificación de discurso, sentimiento
    /// </summary>
    public class OpenAIAnalysisService
    {
        private readonly OpenAIClient _openAIClient;
        private readonly string _deploymentName;
        private readonly ILogger _logger;

        public OpenAIAnalysisService(string endpoint, string apiKey, string deploymentName, ILogger logger)
        {
            _openAIClient = new OpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
            _deploymentName = deploymentName;
            _logger = logger;
        }

        public async Task<AnalysisResult> AnalyzeCallAsync(TranscriptionResult transcription)
        {
            try
            {
                _logger.LogInformation($"Iniciando análisis LLM para CallId: {transcription.CallId}");

                var prompt = BuildAnalysisPrompt(transcription);

                var chatCompletionsOptions = new ChatCompletionsOptions
                {
                    DeploymentName = _deploymentName,
                    Messages =
                    {
                        new ChatRequestSystemMessage(@"Eres un analista experto de call center de ventas a domicilio en México.
                        Analiza transcripciones de llamadas y extrae información clave en formato JSON.
                        
                        Tu análisis debe incluir:
                        1. Motivo de venta perdida (si aplica)
                        2. Citas extraídas (fecha, hora, ubicación, estado)
                        3. Clasificación del manejo del discurso del agente
                        4. Análisis de sentimiento del cliente
                        
                        Responde ÚNICAMENTE con JSON válido sin texto adicional."),
                        new ChatRequestUserMessage(prompt)
                    },
                    Temperature = 0.2f,
                    MaxTokens = 2000
                };

                var response = await _openAIClient.GetChatCompletionsAsync(chatCompletionsOptions);
                var jsonResponse = response.Value.Choices[0].Message.Content;

                // Limpiar respuesta si viene con markdown
                jsonResponse = jsonResponse.Replace("```json", "").Replace("```", "").Trim();

                var analysisResult = JsonSerializer.Deserialize<AnalysisResult>(jsonResponse);

                if (analysisResult != null)
                {
                    analysisResult.CallId = transcription.CallId;
                    analysisResult.AnalysisDate = DateTime.UtcNow;
                }

                _logger.LogInformation($"Análisis completado para CallId: {transcription.CallId}");

                return analysisResult ?? new AnalysisResult { CallId = transcription.CallId };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error en análisis LLM para {transcription.CallId}: {ex.Message}");
                throw;
            }
        }

        private string BuildAnalysisPrompt(TranscriptionResult transcription)
        {
            var transcriptText = string.Join("\n", 
                transcription.Segments.Select(s => $"[{s.StartTime:mm\\:ss}] {s.Speaker}: {s.Text}"));

            return $@"Analiza esta transcripción de llamada de call center de ventas a domicilio:

TRANSCRIPCIÓN:
{transcriptText}

Genera un análisis completo en el siguiente formato JSON exacto:

{{
    ""lostSaleAnalysis"": {{
        ""isLostSale"": true o false,
        ""primaryReason"": ""razón principal si se perdió la venta"",
        ""secondaryReasons"": [""razón 2"", ""razón 3""],
        ""confidenceScore"": 0.0 a 1.0,
        ""reasonCategory"": ""Precio"" o ""Servicio"" o ""Competencia"" o ""NoInteresado"" o ""Otro"",
        ""detailedExplanation"": ""explicación detallada""
    }},
    ""extractedAppointments"": [
        {{
            ""proposedDate"": ""2025-10-20T14:00:00"" o null,
            ""timeSlot"": ""2-4 PM"",
            ""status"": ""Confirmada"" o ""Pendiente"" o ""Rechazada"",
            ""location"": ""dirección o zona"",
            ""notes"": ""notas adicionales""
        }}
    ],
    ""discourseClassification"": {{
        ""agentCommunicationStyle"": ""Profesional"" o ""Agresivo"" o ""Pasivo"" o ""Persuasivo"",
        ""keyPhrases"": [""frase clave 1"", ""frase clave 2""],
        ""objections"": [""objeción del cliente 1""],
        ""closingAttempts"": [""intento de cierre 1""],
        ""followedScript"": true o false,
        ""professionalismScore"": 0.0 a 1.0,
        ""interactionQuality"": ""Excelente"" o ""Buena"" o ""Regular"" o ""Mala""
    }},
    ""sentiment"": {{
        ""overallSentiment"": ""Positive"" o ""Neutral"" o ""Negative"",
        ""sentimentScore"": -1.0 a 1.0,
        ""customerSatisfactionLevel"": ""Muy Satisfecho"" o ""Satisfecho"" o ""Neutral"" o ""Insatisfecho"" o ""Muy Insatisfecho""
    }}
}}";
        }
    }
}