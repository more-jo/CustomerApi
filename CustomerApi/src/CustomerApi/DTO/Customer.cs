namespace CustomerApi;

public record Customer(int Id, string Name) : ICustomer
{
  public bool IsDeleted { get; set; }
}