﻿using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace DealDynamo.Models.ProductViewModels
{
    public class CreateProductViewModel
    {
        public string Title { get; set; }

        [Range(minimum: 1, maximum: 100000)]
        public int Quantity { get; set; }
        public string? Description { get; set; }

        [Display(Name = "Product Image")]
        [DataType(DataType.Upload)]
        public IFormFile ProductImage { get; set; }
        public IEnumerable<Category> Categories { get; set; }

        [Required(ErrorMessage = "Category Must be Selected")]
        public int CategoryId { get; set; }

        [DataType(DataType.Currency)]
        [Range(minimum:1, maximum: Int32.MaxValue)]
        public int Price { get; set; }

    }
}
