using System.ComponentModel.DataAnnotations;

namespace DealDynamo.Models.ReviewViewModels
{
    public class ReviewEditViewModel
    {
        public int ID { get; set; }
        public int ProductID { get; set; }
        public string ProductTitle { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars.")]
        public int Rating { get; set; }
        [Required]
        [StringLength(1000, ErrorMessage = "Comment cannot be longer than 1000 characters.")]
        public string Comment { get; set; }
        [Display(Name = "Date Submitted")]
        public DateTime DateSubmitted { get; set; }
    }
}
