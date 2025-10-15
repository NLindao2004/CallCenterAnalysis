using System;
using System.Collections.Generic;

namespace CallAnalysis.Functions.Models
{
    public class CallRecording
    {
        public string CallId { get; set; } = string.Empty;
        public string BlobUri { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public DateTime CallDate { get; set; }
        public string AgentId { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class TranscriptionResult
    {
        public string CallId { get; set; } = string.Empty;
        public string RawText { get; set; } = string.Empty;
        public List<TranscriptionSegment> Segments { get; set; } = new();
        public DateTime TranscriptionTimestamp { get; set; }
        public string Language { get; set; } = "es-MX";
        public double ConfidenceScore { get; set; }
    }

    public class TranscriptionSegment
    {
        public int SegmentId { get; set; }
        public string Speaker { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public double Confidence { get; set; }
    }

    public class AnalysisResult
    {
        public string CallId { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
        public LostSaleAnalysis LostSaleAnalysis { get; set; } = new();
        public List<Appointment> ExtractedAppointments { get; set; } = new();
        public DiscourseClassification DiscourseClassification { get; set; } = new();
        public SentimentAnalysis Sentiment { get; set; } = new();
    }

    public class LostSaleAnalysis
    {
        public bool IsLostSale { get; set; }
        public string PrimaryReason { get; set; } = string.Empty;
        public List<string> SecondaryReasons { get; set; } = new();
        public double ConfidenceScore { get; set; }
        public string ReasonCategory { get; set; } = string.Empty;
        public string DetailedExplanation { get; set; } = string.Empty;
    }

    public class Appointment
    {
        public DateTime? ProposedDate { get; set; }
        public string TimeSlot { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    public class DiscourseClassification
    {
        public string AgentCommunicationStyle { get; set; } = string.Empty;
        public List<string> KeyPhrases { get; set; } = new();
        public List<string> Objections { get; set; } = new();
        public List<string> ClosingAttempts { get; set; } = new();
        public bool FollowedScript { get; set; }
        public double ProfessionalismScore { get; set; }
        public string InteractionQuality { get; set; } = string.Empty;
    }

    public class SentimentAnalysis
    {
        public string OverallSentiment { get; set; } = string.Empty;
        public double SentimentScore { get; set; }
        public Dictionary<string, double> SegmentSentiments { get; set; } = new();
        public string CustomerSatisfactionLevel { get; set; } = string.Empty;
    }
}