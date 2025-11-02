using System;
using System.Collections.Generic;

namespace AutoEdge.TempModels;

public partial class PurchaseStatusHistory
{
    public int Id { get; set; }

    public int PurchaseId { get; set; }

    public string FromStatus { get; set; } = null!;

    public string ToStatus { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string Notes { get; set; } = null!;

    public string ChangedBy { get; set; } = null!;

    public string ChangeReason { get; set; } = null!;

    public DateTime ChangedDate { get; set; }

    public bool IsSystemGenerated { get; set; }

    public virtual Purchase Purchase { get; set; } = null!;
}
