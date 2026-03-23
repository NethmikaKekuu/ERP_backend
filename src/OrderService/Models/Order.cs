using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("orders")]
public class Order
{
    [Key]
    public Guid Id { get; set; }

    [Column("customer_id")]
    public Guid CustomerId { get; set; }

    public string Status { get; set; } = "PENDING";

    [Column("total_amount")]
    public decimal TotalAmount { get; set; }

    public string Currency { get; set; } = "USD";

    public string? Notes { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    // 🔗 Relationship
    public List<OrderItem> Items { get; set; }
}