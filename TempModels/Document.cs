using System;
using System.Collections.Generic;

namespace AutoEdge.TempModels;

public partial class Document
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public int DocumentTypeId { get; set; }

    public string FileName { get; set; } = null!;

    public string FilePath { get; set; } = null!;

    public long FileSize { get; set; }

    public string? ContentType { get; set; }

    public DateTime UploadDate { get; set; }

    public string Status { get; set; } = null!;

    public string? ValidationNotes { get; set; }

    public string? ReviewedBy { get; set; }

    public DateTime? ReviewDate { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public bool IsActive { get; set; }

    public string? ExtractedText { get; set; }

    public bool IsOcrProcessed { get; set; }

    public DateTime? OcrProcessedDate { get; set; }

    public string? OcrValidationResults { get; set; }

    public string Metadata { get; set; } = null!;

    public string DocumentNumber { get; set; } = null!;

    public DateTime? DocumentIssueDate { get; set; }

    public DateTime? DocumentExpiryDate { get; set; }

    public string IssuingAuthority { get; set; } = null!;

    public bool IsRequired { get; set; }

    public int DisplayOrder { get; set; }

    public string Description { get; set; } = null!;

    public bool IsConfidential { get; set; }

    public string EncryptionKey { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public int? PurchaseId { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual DocumentType DocumentType { get; set; } = null!;

    public virtual Purchase? Purchase { get; set; }

    public virtual AspNetUser? ReviewedByNavigation { get; set; }
}
