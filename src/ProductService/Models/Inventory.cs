using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductService.Models
{
    public class Inventory
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

<<<<<<< HEAD
        [Required]
=======
<<<<<<< Updated upstream
        [Required]
=======
>>>>>>> Stashed changes
>>>>>>> a62bde7 (IMS-17: implement core product management logic and DTOs)
        public Guid ProductId { get; set; }

        public int QuantityAvailable { get; set; } = 0;

        public int QuantityReserved { get; set; } = 0;

        public int LowStockThreshold { get; set; } = 10;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        [NotMapped]
        public bool IsLowStock => QuantityAvailable <= LowStockThreshold;
    }
}
