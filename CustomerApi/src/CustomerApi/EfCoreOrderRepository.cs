namespace CustomerApi;

/// <summary>
/// In-memory repository for local development and testing.
/// Registered as Singleton so all requests share the same data within one app instance.
/// </summary>
public class EfCoreOrderRepository : IOrderRepository
{
    private const bool SUCCESS = true;
    private const bool ERROR = false;

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

    public bool Delete(int id)
    {
        var order = _dbContext.Orders.SingleOrDefault(o => o.Id == id);

        if (order is not null)
        {
            order.IsDeleted = true;
            _dbContext.SaveChanges();

            return SUCCESS;
        }

        return ERROR;
    }

    public int GetMaxId()
    {
        return _dbContext.Orders.Max(o => (int?)o.Id) ?? 0;
    }

    public List<Order> GetOrderByCustomerId(int customerId)
    {
        return _dbContext.Orders.Where(order => order.CustomerId == customerId).ToList();
    }

    public Order GetOrderByOrderId(int orderId)
    {
        return _dbContext.Orders.FirstOrDefault(order => order.Id == orderId);
    }
}
