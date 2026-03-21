using System;
using System.ComponentModel.DataAnnotations;
<<<<<<< HEAD
using System.ComponentModel.DataAnnotations.Schema;
=======
<<<<<<< Updated upstream
using System.ComponentModel.DataAnnotations.Schema;
=======
>>>>>>> Stashed changes
>>>>>>> a62bde7 (IMS-17: implement core product management logic and DTOs)

namespace ProductService.Models
{
    public class LowStockAlert
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

        public int QuantityAtAlert { get; set; }

        public bool IsResolved { get; set; } = false;

        public DateTime AlertedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ResolvedAt { get; set; }

<<<<<<< HEAD
        [ForeignKey("ProductId")]
=======
<<<<<<< Updated upstream
        [ForeignKey("ProductId")]
=======
>>>>>>> Stashed changes
>>>>>>> a62bde7 (IMS-17: implement core product management logic and DTOs)
        public virtual Product? Product { get; set; }
    }
}
