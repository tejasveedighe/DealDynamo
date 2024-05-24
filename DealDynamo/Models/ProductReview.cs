using DealDynamo.Areas.Identity.Data;
using DealDynamo.Models;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
namespace DealDynamo.Models
{
    public class ProductReview
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public int ProductID { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars.")]
        public int Rating { get; set; }

        [Required]
        [StringLength(1000, ErrorMessage = "Comment cannot be longer than 1000 characters.")]
        public string Comment { get; set; }

        [Required]
        public DateTime DateSubmitted { get; set; }

        // Navigation properties
        [JsonIgnore]
        public Product Product { get; set; }
        public ApplicationUser User { get; set; }
    }
}