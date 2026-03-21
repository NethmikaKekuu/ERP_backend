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
    public class InventoryReservation
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

<<<<<<< HEAD
=======
<<<<<<< Updated upstream
>>>>>>> a62bde7 (IMS-17: implement core product management logic and DTOs)
        [Required]
        public Guid ProductId { get; set; }

        [Required]
<<<<<<< HEAD
=======
=======
        public Guid ProductId { get; set; }

>>>>>>> Stashed changes
>>>>>>> a62bde7 (IMS-17: implement core product management logic and DTOs)
        public Guid OrderId { get; set; }

        public int Quantity { get; set; }

        [Required]
        [MaxLength(20)]
<<<<<<< HEAD
        public string Status { get; set; } = "RESERVED"; // RESERVED, RELEASED, FULFILLED
=======
<<<<<<< Updated upstream
        public string Status { get; set; } = "RESERVED"; // RESERVED, RELEASED, FULFILLED
=======
        public string Status { get; set; } = "RESERVED";
>>>>>>> Stashed changes
>>>>>>> a62bde7 (IMS-17: implement core product management logic and DTOs)

        public DateTime ReservedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReleasedAt { get; set; }

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
