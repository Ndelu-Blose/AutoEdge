using System;
using System.Collections.Generic;

namespace AutoEdge.TempModels;

public partial class DocumentType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsRequired { get; set; }

    public bool IsActive { get; set; }

    public string? AllowedFileTypes { get; set; }

    public int MaxFileSizeMb { get; set; }

    public string? Requirements { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
}
