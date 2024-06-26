﻿using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DealDynamo.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public int Quantity { get; set; }
        public string? Description { get; set; }
        [Display(Name = "Product Image")]
        public string ProductImage { get; set; }
        public int? CategoryID { get; set; }
        public Guid SellerID { get; set; }
        public int Price { get; set; }

        // Navigation Property
        [JsonIgnore]
        public ICollection<ProductReview> ProductReviews { get; set; }
    }
}
