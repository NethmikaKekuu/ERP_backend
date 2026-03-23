    namespace OrderService.Services
{
    public class OrderServiceImpl{

    public async Task<Order> CreateOrder(Order order)
    {
        // 🔹 TODO: Call ProductService to check stock

        foreach (var item in order.Items)
        {
            // Example check (replace with API call)
            if (item.Quantity <= 0)
                throw new Exception("Invalid quantity");
        }

        order.Status = "PENDING";

        // 🔹 TODO: Save to DB (EF Core)

        return order;
    }
    public async Task<Order> UpdateStatus(Order order, string newStatus)
    {
        var allowed = new Dictionary<string, List<string>>()
        {
            { "PENDING", new List<string>{ "SHIPPED", "CANCELLED" } },
            { "SHIPPED", new List<string>{ "DELIVERED" } }
        };

        if (!allowed.ContainsKey(order.Status) ||
            !allowed[order.Status].Contains(newStatus))
        {
            throw new Exception("Invalid status transition");
        }

        order.Status = newStatus;
        return order;
    }

    public async Task<Order> CancelOrder(Order order)
    {
        if (order.Status == "CANCELLED")
            throw new Exception("Already cancelled");

        order.Status = "CANCELLED";

        // 🔹 TODO: Restore stock via ProductService

        return order;
    }
}
}
