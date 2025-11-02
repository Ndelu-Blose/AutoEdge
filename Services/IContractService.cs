using AutoEdge.Models.Entities;

namespace AutoEdge.Services
{
    public interface IContractService
    {
        Task<ContractGenerationResult> GenerateContractAsync(Purchase purchase);
        Task<byte[]> GeneratePdfAsync(string contractHtml);
        Task<string> GetContractTemplateAsync(string templateName);
        Task<string> ProcessTemplateAsync(string template, Purchase purchase);
    }

    public class ContractGenerationResult
    {
        public bool Success { get; set; }
        public string ContractHtml { get; set; } = string.Empty;
        public byte[] PdfBytes { get; set; } = Array.Empty<byte>();
        public string ErrorMessage { get; set; } = string.Empty;
        public string ContractNumber { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
    }
}