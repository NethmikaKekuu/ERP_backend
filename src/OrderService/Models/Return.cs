using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("returns")]
public class Return
{
    [Key]
    public Guid Id { get; set; }

    [Column("order_id")]
    public Guid OrderId { get; set; }

    public string? Reason { get; set; }

    public string Status { get; set; }

    [Column("refund_amount")]
    public decimal? RefundAmount { get; set; }
}