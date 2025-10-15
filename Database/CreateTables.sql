-- Script SQL para crear las tablas Gold en SQL Server local

CREATE DATABASE CallAnalytics;
GO

USE CallAnalytics;
GO

-- Tabla principal de análisis
CREATE TABLE GoldCallAnalytics (
    CallId NVARCHAR(50) PRIMARY KEY,
    CallDate DATETIME2 NOT NULL,
    AgentId NVARCHAR(50),
    CustomerId NVARCHAR(50),
    IsLostSale BIT NOT NULL,
    LostSaleReason NVARCHAR(500),
    LostSaleCategory NVARCHAR(100),
    AppointmentsExtracted INT,
    CommunicationStyle NVARCHAR(100),
    ProfessionalismScore DECIMAL(3,2),
    OverallSentiment NVARCHAR(50),
    SentimentScore DECIMAL(3,2),
    InteractionQuality NVARCHAR(50),
    ProcessedAt DATETIME2 NOT NULL,
    PartitionDate DATE NOT NULL,
    INDEX IX_CallDate (CallDate),
    INDEX IX_PartitionDate (PartitionDate),
    INDEX IX_IsLostSale (IsLostSale)
);

-- Tabla de detalles de ventas perdidas
CREATE TABLE LostSaleDetails (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CallId NVARCHAR(50) NOT NULL,
    PrimaryReason NVARCHAR(500),
    SecondaryReasons NVARCHAR(MAX), -- JSON array
    ConfidenceScore DECIMAL(3,2),
    ReasonCategory NVARCHAR(100),
    DetailedExplanation NVARCHAR(MAX),
    FOREIGN KEY (CallId) REFERENCES GoldCallAnalytics(CallId)
);

-- Tabla de citas extraídas
CREATE TABLE ExtractedAppointments (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CallId NVARCHAR(50) NOT NULL,
    ProposedDate DATETIME2,
    TimeSlot NVARCHAR(100),
    Status NVARCHAR(50),
    Location NVARCHAR(500),
    Notes NVARCHAR(MAX),
    FOREIGN KEY (CallId) REFERENCES GoldCallAnalytics(CallId)
);

-- Tabla de clasificación de discurso
CREATE TABLE DiscourseClassification (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CallId NVARCHAR(50) NOT NULL,
    AgentCommunicationStyle NVARCHAR(100),
    KeyPhrases NVARCHAR(MAX), -- JSON array
    Objections NVARCHAR(MAX), -- JSON array
    ClosingAttempts NVARCHAR(MAX), -- JSON array
    FollowedScript BIT,
    ProfessionalismScore DECIMAL(3,2),
    InteractionQuality NVARCHAR(50),
    FOREIGN KEY (CallId) REFERENCES GoldCallAnalytics(CallId)
);

-- Vista para Power BI
CREATE VIEW vw_PowerBI_DailySummary AS
SELECT 
    CAST(CallDate AS DATE) AS Date,
    COUNT(*) AS TotalCalls,
    SUM(CASE WHEN IsLostSale = 1 THEN 1 ELSE 0 END) AS LostSales,
    AVG(ProfessionalismScore) AS AvgProfessionalism,
    AVG(SentimentScore) AS AvgSentiment,
    LostSaleCategory,
    InteractionQuality
FROM GoldCallAnalytics
GROUP BY CAST(CallDate AS DATE), LostSaleCategory, InteractionQuality;
GO

-- Stored Procedure para upsert
CREATE PROCEDURE sp_UpsertCallAnalytics
    @CallId NVARCHAR(50),
    @CallDate DATETIME2,
    @AgentId NVARCHAR(50),
    @CustomerId NVARCHAR(50),
    @IsLostSale BIT,
    @LostSaleReason NVARCHAR(500),
    @LostSaleCategory NVARCHAR(100),
    @AppointmentsExtracted INT,
    @CommunicationStyle NVARCHAR(100),
    @ProfessionalismScore DECIMAL(3,2),
    @OverallSentiment NVARCHAR(50),
    @SentimentScore DECIMAL(3,2),
    @InteractionQuality NVARCHAR(50),
    @ProcessedAt DATETIME2,
    @PartitionDate DATE
AS
BEGIN
    MERGE GoldCallAnalytics AS target
    USING (SELECT @CallId AS CallId) AS source
    ON target.CallId = source.CallId
    WHEN MATCHED THEN
        UPDATE SET
            CallDate = @CallDate,
            AgentId = @AgentId,
            CustomerId = @CustomerId,
            IsLostSale = @IsLostSale,
            LostSaleReason = @LostSaleReason,
            LostSaleCategory = @LostSaleCategory,
            AppointmentsExtracted = @AppointmentsExtracted,
            CommunicationStyle = @CommunicationStyle,
            ProfessionalismScore = @ProfessionalismScore,
            OverallSentiment = @OverallSentiment,
            SentimentScore = @SentimentScore,
            InteractionQuality = @InteractionQuality,
            ProcessedAt = @ProcessedAt,
            PartitionDate = @PartitionDate
    WHEN NOT MATCHED THEN
        INSERT (CallId, CallDate, AgentId, CustomerId, IsLostSale, LostSaleReason, 
                LostSaleCategory, AppointmentsExtracted, CommunicationStyle, 
                ProfessionalismScore, OverallSentiment, SentimentScore, 
                InteractionQuality, ProcessedAt, PartitionDate)
        VALUES (@CallId, @CallDate, @AgentId, @CustomerId, @IsLostSale, @LostSaleReason,
                @LostSaleCategory, @AppointmentsExtracted, @CommunicationStyle,
                @ProfessionalismScore, @OverallSentiment, @SentimentScore,
                @InteractionQuality, @ProcessedAt, @PartitionDate);
END;
GO