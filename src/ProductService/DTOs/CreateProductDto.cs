<<<<<<< HEAD
=======
<<<<<<< Updated upstream
=======
using System;
>>>>>>> Stashed changes
>>>>>>> a62bde7 (IMS-17: implement core product management logic and DTOs)
using System.ComponentModel.DataAnnotations;

namespace ProductService.DTOs
{
<<<<<<< HEAD
=======
<<<<<<< Updated upstream
>>>>>>> a62bde7 (IMS-17: implement core product management logic and DTOs)
    /// <summary>
    /// Data Transfer Object for creating a new product.
    /// </summary>
    public class CreateProductDto
    {
        /// <summary>Stock Keeping Unit — must be unique across all products.</summary>
<<<<<<< HEAD
=======
=======
    public class CreateProductDto
    {
>>>>>>> Stashed changes
>>>>>>> a62bde7 (IMS-17: implement core product management logic and DTOs)
        [Required]
        [MaxLength(100)]
        public string Sku { get; set; } = string.Empty;

<<<<<<< HEAD
        /// <summary>Display name of the product.</summary>
=======
<<<<<<< Updated upstream
        /// <summary>Display name of the product.</summary>
=======
>>>>>>> Stashed changes
>>>>>>> a62bde7 (IMS-17: implement core product management logic and DTOs)
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

<<<<<<< HEAD
=======
<<<<<<< Updated upstream
>>>>>>> a62bde7 (IMS-17: implement core product management logic and DTOs)
        /// <summary>Optional long-form description of the product.</summary>
        public string? Description { get; set; }

        /// <summary>Optional category the product belongs to (FK → Categories.Id).</summary>
        public int? CategoryId { get; set; }

        /// <summary>Unit selling price. Must be greater than 0.</summary>
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        /// <summary>Whether the product is available for ordering. Defaults to true.</summary>
        public bool IsActive { get; set; } = true;

        /// <summary>Initial stock quantity loaded into Inventory. Defaults to 0.</summary>
        [Range(0, int.MaxValue, ErrorMessage = "Initial stock cannot be negative")]
        public int InitialStock { get; set; } = 0;

        /// <summary>Quantity at which a low-stock alert is raised. Defaults to 10.</summary>
<<<<<<< HEAD
=======
=======
        public string? Description { get; set; }

        public int? CategoryId { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        public bool IsActive { get; set; } = true;

        [Range(0, int.MaxValue)]
        public int InitialStock { get; set; } = 0;

>>>>>>> Stashed changes
>>>>>>> a62bde7 (IMS-17: implement core product management logic and DTOs)
        [Range(0, int.MaxValue)]
        public int LowStockThreshold { get; set; } = 10;
    }
}
