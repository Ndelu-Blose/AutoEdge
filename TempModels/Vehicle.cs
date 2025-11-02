using System;
using System.Collections.Generic;

namespace AutoEdge.TempModels;

public partial class Vehicle
{
    public int Id { get; set; }

    public string Vin { get; set; } = null!;

    public string Make { get; set; } = null!;

    public string Model { get; set; } = null!;

    public int Year { get; set; }

    public int Mileage { get; set; }

    public string EngineType { get; set; } = null!;

    public string Transmission { get; set; } = null!;

    public string InteriorColor { get; set; } = null!;

    public string ExteriorColor { get; set; } = null!;

    public string Condition { get; set; } = null!;

    public string Features { get; set; } = null!;

    public decimal CostPrice { get; set; }

    public decimal SellingPrice { get; set; }

    public decimal NegotiableRange { get; set; }

    public string Status { get; set; } = null!;

    public string Location { get; set; } = null!;

    public DateTime DateListed { get; set; }

    public string Source { get; set; } = null!;

    public DateTime PurchaseDate { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public DateTime ModifiedDate { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public string? LicensePlate { get; set; }

    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();

    public virtual ICollection<Inquiry> Inquiries { get; set; } = new List<Inquiry>();

    public virtual ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();

    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

    public virtual ICollection<VehicleImage> VehicleImages { get; set; } = new List<VehicleImage>();
}
