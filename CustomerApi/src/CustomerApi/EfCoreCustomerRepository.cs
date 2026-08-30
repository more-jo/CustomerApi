namespace CustomerApi;

using Microsoft.EntityFrameworkCore;

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

        var updatedCustomer = customer with { Name = newCustomerName };

        _dbContext.Customers.Entry(customer).State = EntityState.Detached;
        _dbContext.Customers.Update(updatedCustomer);

        _dbContext.SaveChanges();

        return SUCCESS;
    }

    public bool Delete(int id)
    {
        var customer = _dbContext.Customers.SingleOrDefault(c => c.Id == id);

        if (customer is not null)
        {
            customer.IsDeleted = true;

            _dbContext.SaveChanges();

            return SUCCESS;
        }

        return ERROR;
    }

    public Result Patch(Customer patchedCustomer)
    {
        var customer = _dbContext.Customers.Find(patchedCustomer.Id);

        if (customer is null)
        {
            return Errors.AccountNotFound;
        }

        _dbContext.Customers.Entry(customer).State = EntityState.Detached;
        _dbContext.Customers.Update(patchedCustomer);

        _dbContext.SaveChanges();

        return Result.Success();
    }
}
