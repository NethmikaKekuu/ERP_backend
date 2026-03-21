using System;
using System.Collections.Generic;
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
    public class Category
    {
        [Key]
<<<<<<< HEAD
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
=======
<<<<<<< Updated upstream
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
=======
>>>>>>> Stashed changes
>>>>>>> a62bde7 (IMS-17: implement core product management logic and DTOs)
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public int? ParentId { get; set; }

        [MaxLength(255)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

<<<<<<< HEAD
=======
<<<<<<< Updated upstream
>>>>>>> a62bde7 (IMS-17: implement core product management logic and DTOs)
        [ForeignKey("ParentId")]
        public virtual Category? Parent { get; set; }
        public virtual ICollection<Category> SubCategories { get; set; } = new List<Category>();

<<<<<<< HEAD
=======
=======
        [System.Text.Json.Serialization.JsonIgnore]
>>>>>>> Stashed changes
>>>>>>> a62bde7 (IMS-17: implement core product management logic and DTOs)
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
