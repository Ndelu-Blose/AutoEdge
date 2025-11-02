using System;
using System.Collections.Generic;

namespace AutoEdge.TempModels;

public partial class Delivery
{
    public int Id { get; set; }

    public int ContractId { get; set; }

    public string DeliveryType { get; set; } = null!;

    public DateTime ScheduledDate { get; set; }

    public DateTime? CompletedDate { get; set; }

    public string DeliveryAddress { get; set; } = null!;

    public string City { get; set; } = null!;

    public string State { get; set; } = null!;

    public string ZipCode { get; set; } = null!;

    public string Country { get; set; } = null!;

    public string? DriverUserId { get; set; }

    public string DriverName { get; set; } = null!;

    public string DriverPhone { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string TrackingNumber { get; set; } = null!;

    public string Notes { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public DateTime ModifiedDate { get; set; }

    public bool IsActive { get; set; }

    public decimal DeliveryFee { get; set; }

    public string DeliveryCompany { get; set; } = null!;

    public string VehicleLicensePlate { get; set; } = null!;

    public DateTime? PickupTime { get; set; }

    public DateTime? EstimatedArrival { get; set; }

    public DateTime? ActualArrival { get; set; }

    public string DeliveryInstructions { get; set; } = null!;

    public string ContactPersonName { get; set; } = null!;

    public string ContactPersonPhone { get; set; } = null!;

    public string ContactPersonEmail { get; set; } = null!;

    public bool RequiresSignature { get; set; }

    public string SignaturePath { get; set; } = null!;

    public string SignedByName { get; set; } = null!;

    public DateTime? SignedDate { get; set; }

    public string DeliveryPhotoPath { get; set; } = null!;

    public string CustomerFeedback { get; set; } = null!;

    public int CustomerRating { get; set; }

    public string FailureReason { get; set; } = null!;

    public int RescheduleCount { get; set; }

    public DateTime? LastRescheduleDate { get; set; }

    public string RescheduleReason { get; set; } = null!;

    public bool IsInsured { get; set; }

    public string InsuranceProvider { get; set; } = null!;

    public string InsurancePolicyNumber { get; set; } = null!;

    public decimal InsuranceAmount { get; set; }

    public decimal? CurrentLatitude { get; set; }

    public decimal? CurrentLongitude { get; set; }

    public DateTime? LastLocationUpdate { get; set; }

    public decimal? DistanceRemaining { get; set; }

    public int? EstimatedMinutesRemaining { get; set; }

    public virtual Contract Contract { get; set; } = null!;

    public virtual AspNetUser? DriverUser { get; set; }
}
