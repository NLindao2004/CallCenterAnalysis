using System;

namespace CallAnalysis.Functions.Models
{
    // Capa Bronce - Datos crudos
    public class BronzeCallRecord
    {
        public string CallId { get; set; } = string.Empty;
        public string BlobPath { get; set; } = string.Empty;
        public DateTime IngestedAt { get; set; }
        public string RawTranscription { get; set; } = string.Empty;
        public string MetadataJson { get; set; } = string.Empty;
    }

    // Capa Plata - Datos limpios
    public class SilverCallRecord
    {
        public string CallId { get; set; } = string.Empty;
        public DateTime CallDate { get; set; }
        public string CleanedTranscription { get; set; } = string.Empty;
        public string NormalizedText { get; set; } = string.Empty;
        public int WordCount { get; set; }
        public double Duration { get; set; }
        public string ProcessedAt { get; set; } = string.Empty;
    }

    // Capa Oro - Datos analíticos
    public class GoldCallAnalytics
    {
        public string CallId { get; set; } = string.Empty;
        public DateTime CallDate { get; set; }
        public string AgentId { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public bool IsLostSale { get; set; }
        public string LostSaleReason { get; set; } = string.Empty;
        public string LostSaleCategory { get; set; } = string.Empty;
        public int AppointmentsExtracted { get; set; }
        public string CommunicationStyle { get; set; } = string.Empty;
        public double ProfessionalismScore { get; set; }
        public string OverallSentiment { get; set; } = string.Empty;
        public double SentimentScore { get; set; }
        public string InteractionQuality { get; set; } = string.Empty;
        public DateTime ProcessedAt { get; set; }
        public string PartitionDate { get; set; } = string.Empty;
    }
}