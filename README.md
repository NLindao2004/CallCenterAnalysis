# 📞 Call Center Analysis System

**Sistema de Análisis de Llamadas para Call Center - Detección de Ventas Perdidas mediante Transcripción y Análisis LLM**

[![.NET 6](https://img.shields.io/badge/.NET-6.0-512BD4)](https://dotnet.microsoft.com/download/dotnet/6.0)
[![Azure Functions](https://img.shields.io/badge/Azure-Functions-0062AD)](https://azure.microsoft.com/services/functions/)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)

---

## 📋 Resumen Ejecutivo

Sistema automatizado para analizar grabaciones de llamadas de call center de ventas a domicilio, detectando causas de ventas perdidas, extrayendo citas y clasificando la calidad del discurso del agente.

### 🎯 Objetivo
Detectar causas de **ventas perdidas** en el call center a partir de grabaciones de llamadas mediante transcripción automática y análisis con LLM.

### 🔄 Modo
**Batch D-1** (proceso nocturno)

### 🔐 Seguridad
- VPN site-to-site
- Private Endpoints (sin exposición pública)

### 📦 Carga
- **AzCopy**: Sube audios a Blob Storage
- **Volumen**: 1,200 horas/mes → 35-70 GB/mes de audio

### 📊 Resultado
- Transcripción + análisis LLM
- **Tablas Oro** en BD local para **Power BI/Analítica**

---

## 🏗️ Arquitectura (Alto Nivel)

```
┌─────────────────────────────────────────────────────────────────┐
│  1. AzCopy (on-prem) → Blob (raw/) por VPN + Private Endpoint  │
│  2. ADF (00:30) orquesta: Speech Batch (diarización, timestamps)│
│  3. Functions/Data Flow: limpieza/normalización/troceo          │
│  4. Azure OpenAI (o4): análisis por lote → JSON                │
│  5. Data Lake Gen2 (Parquet): Bronce / Plata / Oro             │
│  6. ADF + Self-Hosted IR: Oro → SQL local (upsert)             │
│  7. Power BI: KPIs diarios para operación                      │
└─────────────────────────────────────────────────────────────────┘
```

### Componentes Principales

1. **AzCopy (on-prem)** → Blob Storage (raw/) por VPN + Private Endpoint
2. **ADF (00:30)** → Speech Batch API (diarización + timestamps)
3. **Functions/Data Flow** → Limpieza/normalización/troceo
4. **Azure OpenAI (GPT-4)** → Análisis por lote → JSON (motivo, citas, sentimiento)
5. **Data Lake Gen2** → Parquet (Bronze/Silver/Gold)
6. **ADF + Self-Hosted IR** → SQL local (upsert)
7. **Power BI** → KPIs diarios

---

## 📦 Alcance del MVP

### ✅ En Alcance

- [x] Procesamiento batch diario de ~1,200 horas/mes de llamadas
- [x] Clasificar motivo de venta perdida
- [x] Extraer citas programadas
- [x] Clasificación de manejo del discurso del agente
- [x] Entrega de información procesada, filtrada y validada en tablas BD local

### ❌ Fuera de Alcance

- [ ] Tiempo real/streaming
- [ ] Custom Speech entrenado
- [ ] Análisis multimodal (screen recording)
- [ ] Reentrenamiento de LLM
- [ ] Integración IVR/CTI avanzada

---

## 🛠️ Tecnologías

| Componente | Tecnología |
|------------|-----------|
| **Runtime** | .NET 6.0 |
| **Compute** | Azure Functions v4 (Isolated) |
| **Transcripción** | Azure Speech Service (Batch API) |
| **Análisis LLM** | Azure OpenAI (GPT-4) |
| **Almacenamiento** | Azure Data Lake Gen2 |
| **Base de Datos** | SQL Server (on-premises) |
| **Orquestación** | Azure Data Factory |
| **BI** | Power BI |
| **Monitoreo** | Application Insights |

---

## 📂 Estructura del Proyecto

```
CallCenterAnalysis/
├── CallAnalysis.Functions/
│   ├── Functions/
│   │   └── BatchOrchestratorFunction.cs    # Orquestador principal (Timer 00:30)
│   ├── Models/
│   │   ├── CallModels.cs                   # CallRecording, TranscriptionResult, AnalysisResult
│   │   └── DataLakeModels.cs               # Bronze, Silver, Gold records
│   ├── Services/
│   │   ├── SpeechBatchService.cs           # Integración con Speech API
│   │   ├── OpenAIAnalysisService.cs        # Análisis con GPT-4
│   │   ├── DataLakeService.cs              # Gestión de capas Bronze/Silver/Gold
│   │   └── TextCleaningService.cs          # Limpieza y normalización
│   ├── host.json
│   ├── local.settings.json
│   └── Program.cs
├── Database/
│   └── CreateTables.sql                    # Scripts SQL para tablas Gold
├── Docs/
│   ├── ARCHITECTURE.md
│   └── DEPLOYMENT.md
└── README.md
```

---

## 🚀 Inicio Rápido

### Prerequisitos

- ✅ .NET 6.0 SDK
- ✅ Azure Functions Core Tools v4
- ✅ Azure Subscription
- ✅ SQL Server (local o remoto)

### 1. Clonar el repositorio

```bash
git clone https://github.com/NLindao2004/CallCenterAnalysis.git
cd CallCenterAnalysis
```

### 2. Configurar variables de entorno

Editar `CallAnalysis.Functions/local.settings.json`:

```json
{
  "Values": {
    "AzureWebJobsStorage": "YOUR-STORAGE-CONNECTION",
    "SpeechService_SubscriptionKey": "YOUR-SPEECH-KEY",
    "SpeechService_Region": "eastus",
    "OpenAI_Endpoint": "https://YOUR-OPENAI.openai.azure.com/",
    "OpenAI_Key": "YOUR-OPENAI-KEY",
    "OpenAI_DeploymentName": "gpt-4",
    "DataLake_ConnectionString": "YOUR-DATALAKE-CONNECTION",
    "SqlServer_ConnectionString": "Server=localhost;Database=CallAnalytics;..."
  }
}
```

### 3. Crear base de datos

```bash
sqlcmd -S localhost -U sa -P YourPassword -i Database/CreateTables.sql
```

### 4. Ejecutar localmente

```bash
cd CallAnalysis.Functions
func start
```

### 5. Desplegar a Azure

```bash
func azure functionapp publish <YOUR-FUNCTION-APP-NAME>
```

---

## 📊 Análisis Generado

El sistema genera análisis completo en 4 dimensiones:

### 1️⃣ Ventas Perdidas
- ✅ Identificación de venta perdida (sí/no)
- ✅ Motivo primario y secundarios
- ✅ Categorización: Precio, Servicio, Competencia, No Interesado, Otro
- ✅ Nivel de confianza (0-1)
- ✅ Explicación detallada

### 2️⃣ Citas Extraídas
- 📅 Fechas propuestas
- ⏰ Franjas horarias
- ✅ Estado: Confirmada, Pendiente, Rechazada
- 📍 Ubicación
- 📝 Notas adicionales

### 3️⃣ Clasificación de Discurso
- 🎙️ Estilo de comunicación del agente
- 🔑 Frases clave identificadas
- ❌ Objeciones del cliente
- 🎯 Intentos de cierre
- 📋 Cumplimiento de script
- ⭐ Score de profesionalismo (0-1)
- 📈 Calidad de interacción: Excelente, Buena, Regular, Mala

### 4️⃣ Sentimiento
- 😊 Sentimiento general: Positive, Neutral, Negative
- 📊 Score de sentimiento (-1 a 1)
- 👤 Nivel de satisfacción del cliente

---

## 📈 Volumen y Performance

| Métrica | Valor |
|---------|-------|
| **Llamadas/mes** | ~1,200 horas |
| **Tamaño datos** | 35-70 GB/mes audio |
| **Procesamiento** | Nocturno (00:30 AM) |
| **Concurrencia** | 50 actividades paralelas |
| **Batch size** | 50 llamadas por lote |

---

## 🗄️ Modelo de Datos

### Capa ORO (Gold) - SQL Server

```sql
GoldCallAnalytics
├── CallId (PK)
├── CallDate
├── AgentId
├── CustomerId
├── IsLostSale
├── LostSaleReason
├── LostSaleCategory
├── AppointmentsExtracted
├── CommunicationStyle
├── ProfessionalismScore
├── OverallSentiment
├── SentimentScore
├── InteractionQuality
├── ProcessedAt
└── PartitionDate
```

### Tablas Relacionadas

- `LostSaleDetails`: Detalles de ventas perdidas
- `ExtractedAppointments`: Citas extraídas
- `DiscourseClassification`: Clasificación de discurso

---

## 🔧 Configuración de Azure

Ver guía completa en [`Docs/DEPLOYMENT.md`](Docs/DEPLOYMENT.md)

### Recursos Azure Necesarios

- Azure Function App (Consumption o Premium)
- Azure Storage Account (Data Lake Gen2)
- Azure Cognitive Services - Speech
- Azure OpenAI Service
- Azure Data Factory
- Application Insights
- Virtual Network + VPN Gateway
- Private Endpoints

---

## 📝 Logs y Monitoreo

### Application Insights

```bash
# Ver logs en tiempo real
func azure functionapp logstream <YOUR-FUNCTION-APP-NAME>
```

### Power BI Dashboard

KPIs diarios:
- 📊 Total de llamadas procesadas
- 📉 % de ventas perdidas
- 🏆 Top 5 motivos de pérdida
- ⭐ Score promedio de profesionalismo
- 😊 Sentimiento promedio
- 📅 Citas confirmadas vs rechazadas

---

## 🤝 Contribución

Las contribuciones son bienvenidas. Por favor:

1. Fork el proyecto
2. Crea una rama feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abre un Pull Request

---

## 📄 Licencia

Este proyecto está bajo la licencia MIT. Ver archivo `LICENSE` para más detalles.

---

## 👤 Autor

**NLindao2004**

- GitHub: [@NLindao2004](https://github.com/NLindao2004)

---

## 📞 Soporte

Para preguntas o soporte:
- 📫 Crear un issue en GitHub
- 📧 Contactar al equipo de desarrollo

---

**Desarrollado con ❤️ para DIFARE GRUPO**