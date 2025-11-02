using System;
using System.Collections.Generic;

namespace AutoEdge.TempModels;

public partial class Customer
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public string CustomerType { get; set; } = null!;

    public int? CreditScore { get; set; }

    public string PreferredContact { get; set; } = null!;

    public string DeliveryAddress { get; set; } = null!;

    public string BillingAddress { get; set; } = null!;

    public string EmergencyContactName { get; set; } = null!;

    public string EmergencyContactPhone { get; set; } = null!;

    public string EmergencyContactRelation { get; set; } = null!;

    public string Employer { get; set; } = null!;

    public string JobTitle { get; set; } = null!;

    public decimal? AnnualIncome { get; set; }

    public string Notes { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public DateTime ModifiedDate { get; set; }

    public bool IsActive { get; set; }

    public string CustomerStatus { get; set; } = null!;

    public DateTime? LastPurchaseDate { get; set; }

    public int TotalPurchases { get; set; }

    public decimal TotalSpent { get; set; }

    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

    public virtual ICollection<Inquiry> Inquiries { get; set; } = new List<Inquiry>();

    public virtual ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();

    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

    public virtual AspNetUser User { get; set; } = null!;
}
