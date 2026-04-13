namespace CustomerService.Models
{
    public class CustomerOrderReferenceItem
    {
        public int Id { get; set; }
        public int CustomerOrderReferenceId { get; set; }
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }

        public CustomerOrderReference? CustomerOrderReference { get; set; }
    }
}