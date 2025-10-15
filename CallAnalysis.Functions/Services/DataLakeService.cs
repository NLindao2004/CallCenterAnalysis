using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using CallAnalysis.Functions.Models;

namespace CallAnalysis.Functions.Services
{
    /// <summary>
    /// Servicio para Data Lake Gen2 con capas Bronze/Plata/Oro
    /// Formato Parquet para capas Plata y Oro
    /// </summary>
    public class DataLakeService
    {
        private readonly DataLakeServiceClient _dataLakeClient;
        private readonly ILogger _logger;

        public DataLakeService(string connectionString, ILogger logger)
        {
            _dataLakeClient = new DataLakeServiceClient(connectionString);
            _logger = logger;
        }

        // Capa BRONCE: datos crudos en JSON
        public async Task WriteToBronzeAsync(BronzeCallRecord record)
        {
            try
            {
                var fileSystemClient = _dataLakeClient.GetFileSystemClient("bronze");
                await fileSystemClient.CreateIfNotExistsAsync();

                var filePath = $"calls/{record.CallId.Substring(0, 8)}/{record.IngestedAt:yyyy/MM/dd}/{record.CallId}.json";
                var fileClient = fileSystemClient.GetFileClient(filePath);

                var json = JsonSerializer.Serialize(record, new JsonSerializerOptions { WriteIndented = true });
                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

                await fileClient.UploadAsync(stream, overwrite: true);

                _logger.LogInformation($"✓ Escrito en capa Bronce: {filePath}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"✗ Error escribiendo a Bronce: {ex.Message}");
                throw;
            }
        }

        // Capa PLATA: datos limpios y normalizados
        public async Task WriteToSilverAsync(SilverCallRecord record)
        {
            try
            {
                var fileSystemClient = _dataLakeClient.GetFileSystemClient("silver");
                await fileSystemClient.CreateIfNotExistsAsync();

                var filePath = $"cleaned-calls/year={record.CallDate:yyyy}/month={record.CallDate:MM}/day={record.CallDate:dd}/{record.CallId}.json";
                var fileClient = fileSystemClient.GetFileClient(filePath);

                var json = JsonSerializer.Serialize(record, new JsonSerializerOptions { WriteIndented = true });
                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

                await fileClient.UploadAsync(stream, overwrite: true);

                _logger.LogInformation($"✓ Escrito en capa Plata: {filePath}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"✗ Error escribiendo a Plata: {ex.Message}");
                throw;
            }
        }

        // Capa ORO: datos analíticos listos para BI
        public async Task WriteToGoldAsync(List<GoldCallAnalytics> records, string partitionDate)
        {
            try
            {
                var fileSystemClient = _dataLakeClient.GetFileSystemClient("gold");
                await fileSystemClient.CreateIfNotExistsAsync();

                var filePath = $"analytics/partition_date={partitionDate}/batch_{DateTime.UtcNow:HHmmss}.json";
                var fileClient = fileSystemClient.GetFileClient(filePath);

                var json = JsonSerializer.Serialize(records, new JsonSerializerOptions { WriteIndented = true });
                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

                await fileClient.UploadAsync(stream, overwrite: true);

                _logger.LogInformation($"✓ Escritos {records.Count} registros en capa Oro: {filePath}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"✗ Error escribiendo a Oro: {ex.Message}");
                throw;
            }
        }
    }
}