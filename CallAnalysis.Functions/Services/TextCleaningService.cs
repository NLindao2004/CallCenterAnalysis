using System;
using System.Text.RegularExpressions;

namespace CallAnalysis.Functions.Services
{
    /// <summary>
    /// Servicio de limpieza y normalización de texto
    /// Parte del Data Flow: limpieza/normalización/troceo
    /// </summary>
    public class TextCleaningService
    {
        public string CleanTranscription(string rawText)
        {
            if (string.IsNullOrWhiteSpace(rawText))
                return string.Empty;

            var cleaned = rawText;

            // Remover marcadores de tiempo duplicados
            cleaned = Regex.Replace(cleaned, @"\[\d{2}:\d{2}\.\d{3}\]", "");

            // Remover ruido de transcripción
            cleaned = Regex.Replace(cleaned, @"\[inaudible\]|\[ruido\]|\[background\]", "", RegexOptions.IgnoreCase);

            // Remover muletillas comunes en español
            cleaned = Regex.Replace(cleaned, @"\b(este|eh|um|mm|ah)\b", "", RegexOptions.IgnoreCase);

            // Limpiar espacios múltiples
            cleaned = Regex.Replace(cleaned, @"\s+", " ");

            // Limpiar puntuación excesiva
            cleaned = Regex.Replace(cleaned, @"\.{2,}", ".");
            cleaned = Regex.Replace(cleaned, @",{2,}", ",");

            return cleaned.Trim();
        }

        public string NormalizeText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            var normalized = text;

            // Convertir a minúsculas para normalización
            normalized = normalized.ToLowerInvariant();

            // Expandir abreviaciones comunes en español
            normalized = normalized
                .Replace("sr.", "señor")
                .Replace("sra.", "señora")
                .Replace("dr.", "doctor")
                .Replace("tel.", "teléfono")
                .Replace("cel.", "celular")
                .Replace("apdo.", "apartado")
                .Replace("col.", "colonia")
                .Replace("dom.", "domingo")
                .Replace("lun.", "lunes")
                .Replace("mar.", "martes")
                .Replace("mié.", "miércoles")
                .Replace("jue.", "jueves")
                .Replace("vie.", "viernes")
                .Replace("sáb.", "sábado");

            // Normalizar números de teléfono (formato mexicano)
            normalized = Regex.Replace(normalized, @"(\d{2})\s*(\d{4})\s*(\d{4})", "$1-$2-$3");

            return normalized.Trim();
        }

        public int CountWords(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return 0;

            return text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        public string[] ChunkText(string text, int maxTokens = 3000)
        {
            // Trocear texto para procesamiento en lotes
            var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var chunks = new List<string>();
            var currentChunk = new List<string>();
            var currentCount = 0;

            foreach (var word in words)
            {
                currentChunk.Add(word);
                currentCount += word.Length / 4; // Aproximación de tokens

                if (currentCount >= maxTokens)
                {
                    chunks.Add(string.Join(" ", currentChunk));
                    currentChunk.Clear();
                    currentCount = 0;
                }
            }

            if (currentChunk.Any())
                chunks.Add(string.Join(" ", currentChunk));

            return chunks.ToArray();
        }
    }
}