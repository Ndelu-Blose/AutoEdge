using System.ComponentModel.DataAnnotations;

namespace AutoEdge.Models.Entities
{
    public class DocumentType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public bool IsRequired { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        [StringLength(200)]
        public string? AllowedFileTypes { get; set; } // e.g., ".pdf,.jpg,.png"

        public int MaxFileSizeMB { get; set; } = 10;
        
        public long MaxFileSizeBytes => MaxFileSizeMB * 1024 * 1024;
        
        [StringLength(1000)]
        public string? Requirements { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedDate { get; set; }

        // Navigation properties
        public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
    }
}