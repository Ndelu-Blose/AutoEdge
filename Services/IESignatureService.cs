using AutoEdge.Models;

namespace AutoEdge.Services
{
    public interface IESignatureService
    {
        /// <summary>
        /// Sends a contract document to OpenSign for e-signature
        /// </summary>
        /// <param name="contractId">The contract ID</param>
        /// <param name="signerEmail">Email of the person who needs to sign</param>
        /// <param name="signerName">Name of the person who needs to sign</param>
        /// <param name="signerPhone">Phone number of the signer (optional)</param>
        /// <returns>OpenSign document ID and signing URL</returns>
        Task<(string DocumentId, string SigningUrl)> SendForSignatureAsync(int contractId, string signerEmail, string signerName, string? signerPhone = null);

        /// <summary>
        /// Gets the current status of a document from OpenSign
        /// </summary>
        /// <param name="documentId">OpenSign document ID</param>
        /// <returns>Document status information</returns>
        Task<ESignatureStatus> GetDocumentStatusAsync(string documentId);

        /// <summary>
        /// Downloads the signed document from OpenSign
        /// </summary>
        /// <param name="documentId">OpenSign document ID</param>
        /// <returns>Signed document as byte array</returns>
        Task<byte[]> DownloadSignedDocumentAsync(string documentId);

        /// <summary>
        /// Resends signing notification email to the signer
        /// </summary>
        /// <param name="documentId">OpenSign document ID</param>
        /// <returns>Success status</returns>
        Task<bool> ResendSigningNotificationAsync(string documentId);

        /// <summary>
        /// Processes webhook notifications from OpenSign
        /// </summary>
        /// <param name="webhookData">Webhook payload from OpenSign</param>
        /// <returns>Processing result</returns>
        Task<bool> ProcessWebhookAsync(OpenSignWebhookData webhookData);
    }

    public class ESignatureStatus
    {
        public string Status { get; set; } = string.Empty;
        public DateTime? SignedAt { get; set; }
        public string? SignerEmail { get; set; }
        public string? CertificateUrl { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsDeclined { get; set; }
        public string? DeclineReason { get; set; }
    }

    public class OpenSignWebhookData
    {
        public string Event { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string ObjectId { get; set; } = string.Empty;
        public string File { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<OpenSignSigner> Signers { get; set; } = new();
        public string? ViewedBy { get; set; }
        public DateTime? ViewedAt { get; set; }
        public DateTime? SignedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? DeclinedAt { get; set; }
        public string? DeclinedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Certificate { get; set; }
        public OpenSignSigner? Signer { get; set; }
    }

    public class OpenSignSigner
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Url { get; set; }
    }
}