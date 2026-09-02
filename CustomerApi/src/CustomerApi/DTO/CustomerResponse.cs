namespace CustomerApi;

public record CustomerResponse(int Id, string Name, bool IsDeleted) : IHasCustomerName
{
  public static CustomerResponse From(Customer customer) => new(customer.Id, customer.Name, customer.IsDeleted);
}