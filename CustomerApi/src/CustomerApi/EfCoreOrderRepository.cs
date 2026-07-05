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

    public void Add(Order newOrder)
    {
        _dbContext.Orders.Add(newOrder);

        _dbContext.SaveChanges();
    }

    public int GetMaxId()
    {
        return _dbContext.Orders.Max(o => (int?)o.Id) ?? 0;
    }

    public List<Order> GetOrderByCustomerId(int customerId)
    {
        return _dbContext.Orders.Where(order => order.CustomerId == customerId).ToList();
    }
}
