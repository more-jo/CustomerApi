namespace CustomerApi;

/// <summary>
/// In-memory repository for local development and testing.
/// Registered as Singleton so all requests share the same data within one app instance.
/// </summary>
public class EfCoreOrderRepository : IOrderRepository
{
    private CustomerDbContext _dbContext;

    public EfCoreOrderRepository(CustomerDbContext database)
    {
        _dbContext = database;
    }

    public List<Order> GetOrderByCustomerId(int customerId)
    {
        return _dbContext.Orders.Where(customer => customer.Id == customerId).ToList();
    }
}
