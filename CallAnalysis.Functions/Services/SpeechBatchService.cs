using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CallAnalysis.Functions.Services
{
    /// <summary>
    /// Servicio para Speech Batch API con diarización y timestamps
    /// según especificación: ADF (00:30) orquesta Speech Batch
    /// </summary>
    public class SpeechBatchService
    {
        private readonly string _subscriptionKey;
        private readonly string _region;
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;

        public SpeechBatchService(string subscriptionKey, string region, ILogger logger)
        {
            _subscriptionKey = subscriptionKey;
            _region = region;
            _logger = logger;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _subscriptionKey);
        }

        public async Task<string> SubmitBatchTranscriptionAsync(string blobSasUrl, string callId)
        {
            try
            {
                _logger.LogInformation($"Enviando solicitud de transcripción batch para CallId: {callId}");

                var endpoint = $"https://{_region}.api.cognitive.microsoft.com/speechtotext/v3.1/transcriptions";

                var requestBody = new
                {
                    contentUrls = new[] { blobSasUrl },
                    locale = "es-MX",
                    displayName = $"CallCenter-{callId}",
                    properties = new
                    {
                        diarizationEnabled = true,
                        wordLevelTimestampsEnabled = true,
                        punctuationMode = "DictatedAndAutomatic",
                        profanityFilterMode = "Masked"
                    }
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync(endpoint, content);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonDocument>(responseBody);
                var transcriptionId = result?.RootElement.GetProperty("self").GetString()?.Split('/').Last();

                _logger.LogInformation($"Transcripción iniciada. ID: {transcriptionId}");

                return transcriptionId ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al enviar transcripción batch: {ex.Message}");
                throw;
            }
        }

        public async Task<string?> GetTranscriptionStatusAsync(string transcriptionId)
        {
            var endpoint = $"https://{_region}.api.cognitive.microsoft.com/speechtotext/v3.1/transcriptions/{transcriptionId}";
            var response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonDocument>(responseBody);
            return result?.RootElement.GetProperty("status").GetString();
        }
    }
}