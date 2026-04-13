namespace CustomerService.Models
{
    public class CustomerOrderReference
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string ErpOrderId { get; set; } = string.Empty;
        public string OrderNumber { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = "Pending";
        public string LatestStatus { get; set; } = "Created";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Customer? Customer { get; set; }
        public ICollection<CustomerOrderReferenceItem> Items { get; set; } = new List<CustomerOrderReferenceItem>();
    }
}