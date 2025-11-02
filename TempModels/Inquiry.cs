using System;
using System.Collections.Generic;

namespace AutoEdge.TempModels;

public partial class Inquiry
{
    public int Id { get; set; }

    public int VehicleId { get; set; }

    public int CustomerId { get; set; }

    public string InquiryType { get; set; } = null!;

    public string Message { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string Priority { get; set; } = null!;

    public string? AssignedToUserId { get; set; }

    public string Response { get; set; } = null!;

    public DateTime? ResponseDate { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime ModifiedDate { get; set; }

    public bool IsActive { get; set; }

    public string CustomerName { get; set; } = null!;

    public string CustomerEmail { get; set; } = null!;

    public string CustomerPhone { get; set; } = null!;

    public string PreferredContactMethod { get; set; } = null!;

    public DateTime? PreferredContactTime { get; set; }

    public string SpecialRequests { get; set; } = null!;

    public bool IsTestDriveRequested { get; set; }

    public DateTime? PreferredTestDriveDate { get; set; }

    public bool IsFinancingInquiry { get; set; }

    public decimal? OfferedPrice { get; set; }

    public virtual AspNetUser? AssignedToUser { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual Vehicle Vehicle { get; set; } = null!;
}
