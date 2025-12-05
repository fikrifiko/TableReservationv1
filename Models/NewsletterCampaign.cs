using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Table_Reservation.Models
{
    public class NewsletterCampaign
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string CampaignType { get; set; } = string.Empty; // "Newsletter" or "Promotion"

        [MaxLength(50)]
        public string? CouponCode { get; set; }

        public decimal? DiscountPercentage { get; set; }

        public DateTime? ValidUntil { get; set; }

        public int TotalRecipients { get; set; }

        public int SentCount { get; set; }

        public int FailedCount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? SentAt { get; set; }

        [MaxLength(100)]
        public string CreatedBy { get; set; } = string.Empty; // Admin username
    }
}

