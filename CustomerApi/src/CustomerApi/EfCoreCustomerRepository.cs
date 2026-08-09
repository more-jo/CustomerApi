namespace CustomerApi;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// In-memory repository for local development and testing.
/// Seeded with initial customers (Alice, Bob).
/// Registered as Singleton so all requests share the same data within one app instance.
/// </summary>
public class EfCoreCustomerRepository : ICustomerRepository
{
    private const bool SUCCESS = true;
    private const bool ERROR = false;

    private CustomerDbContext _dbContext;

    public EfCoreCustomerRepository(CustomerDbContext database)
    {
        _dbContext = database;
    }

    public List<Customer> GetAll()
    {
        var customers = _dbContext.Customers.ToList();
        return customers;
    }

    public Customer? GetCustomerById(int id) => _dbContext.Customers.Find(id);

    public int GetMaxId()
    {
        return _dbContext.Customers.Max(c => (int?)c.Id) ?? 0;
    }

    public void Add(Customer newCustomer)
    {
        var customer = _dbContext.Customers.SingleOrDefault(c => c.Id == newCustomer.Id);
        if (customer is null)
        {
            _dbContext.Customers.Add(newCustomer);
            _dbContext.SaveChanges();
        }
    }

    public bool Update(int id, string newCustomerName)
    {
        var customer = _dbContext.Customers.Find(id);

        if (customer is null)
        {
            return ERROR;
        }

        // var updatedCustomer = customer with { Name = newCustomerName };
        // _dbContext.Customers.Update(updatedCustomer);

        // Alternative:
        // _dbContext.Customers
        //     .Where(c => c.Id == id)
        //     .ExecuteUpdateAsync(setters => setters
        //     .SetProperty(c => c.Name, newCustomerName));

        var updatedCustomer = customer with { Name = newCustomerName };
        // _dbContext.Customers.Entry(updatedCustomer); // does not work. Design decision to stick to record.
        // _dbContext.Customers.Update(updatedCustomer);

        _dbContext.Customers.Entry(customer).State = EntityState.Detached;
        // _dbContext.Customers.Entry(updatedCustomer).State = EntityState.Modified;
        _dbContext.Customers.Update(updatedCustomer);

        _dbContext.SaveChanges();

        return SUCCESS;
    }

    public bool Delete(int id)
    {
        var customer = _dbContext.Customers.SingleOrDefault(c => c.Id == id);

        if (customer is not null)
        {
            // _dbContext.Customers.Remove(customer);
            customer.IsDeleted = true;

            _dbContext.SaveChanges();

            return SUCCESS;
        }

        return ERROR;
    }
}
