namespace CustomerApi;

/// <summary>
/// In-memory repository for local development and testing.
/// Seeded with initial customers (Alice, Bob).
/// Registered as Singleton so all requests share the same data within one app instance.
/// </summary>
public class InMemoryCustomerRepository : ICustomerRepository
{
    private const bool SUCCESS = true;
    private const bool ERROR = false;

    private readonly List<Customer> _customers = new()
    {
        new Customer(1, "Alice"),
        new Customer(2, "Bob")
    };

    public List<Customer> GetAll() => _customers;

    public Customer? GetCustomerById(int id) => _customers.FirstOrDefault(c => c.Id == id);

    public void Add(Customer customer)
    {
        _customers.Add(customer);
    }

    public bool Update(int id, string newCustomerName)
    {

        var index = _customers.FindIndex(0, _customers.Count, c => c.Id == id);
        if (index >= 0)
        {
            _customers[index] = new Customer(id, newCustomerName);
            return SUCCESS;
        }

        return ERROR;
    }

    public bool Delete(int id)
    {
        var index = _customers.FindIndex(0, _customers.Count, c => c.Id == id);
        if (index >= 0)
        {
            _customers.RemoveAt(index);
            return SUCCESS;
        }

        return ERROR;
    }

    public int GetMaxId()
    {
        return _customers.Max(c => c.Id);
    }
}