using System;
using System.Collections.Generic;

namespace AutoEdge.TempModels;

public partial class VehicleImage
{
    public int Id { get; set; }

    public int VehicleId { get; set; }

    public string ImagePath { get; set; } = null!;

    public string ImageType { get; set; } = null!;

    public DateTime UploadDate { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; }

    public string FileName { get; set; } = null!;

    public long FileSize { get; set; }

    public string ContentType { get; set; } = null!;

    public string AltText { get; set; } = null!;

    public virtual Vehicle Vehicle { get; set; } = null!;
}
