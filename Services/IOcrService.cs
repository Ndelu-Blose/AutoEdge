using AutoEdge.Models;
using AutoEdge.Models.Entities;

namespace AutoEdge.Services
{
    public interface IOcrService
    {
        Task<OcrResult> ExtractTextAsync(string filePath, DocumentType documentType);
        Task<ValidationResult> ValidateDocumentAsync(string extractedText, DocumentType documentType);
    }

    public class OcrResult
    {
        public bool Success { get; set; }
        public string ExtractedText { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public Dictionary<string, string> ExtractedFields { get; set; } = new();
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> ValidationErrors { get; set; } = new();
        public Dictionary<string, object> ValidatedFields { get; set; } = new();
        public double ConfidenceScore { get; set; }
    }
}