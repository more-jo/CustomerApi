namespace CustomerApi;

/// <summary>
/// Repository abstraction for customer data access.
/// Start minimal: add methods as you refactor each endpoint.
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
    /// Delete one customer on position.
    /// </summary>
    bool Delete(int id);
}
