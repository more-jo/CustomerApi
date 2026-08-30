namespace CustomerApi;

/// <summary>
/// Repository abstraction for customer data access.
/// </summary>
public interface ICustomerRepository
{
    /// <summary>
    /// Get all customers.
    /// </summary>
    List<Customer> GetAll();

    /// <summary>
    /// Get highest ID.
    /// </summary>
    int GetMaxId();

    /// <summary>
    /// Get one single customer based on the id.
    /// </summary>
    Customer? GetCustomerById(int id);

    /// <summary>
    /// Adds one customer.
    /// </summary>
    void Add(Customer customer);

    /// <summary>
    /// Updates one customer.
    /// </summary>
    bool Update(int id, string newCustomerName);

    /// <summary>
    /// Marks the customer as deleted (soft delete). The record remains retrievable.
    /// Returns bool for presence of customer.
    /// False if absent.
    /// </summary>
    bool Delete(int id);

    Result Patch(Customer customer);
}
