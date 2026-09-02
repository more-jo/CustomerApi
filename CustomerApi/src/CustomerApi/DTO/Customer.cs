namespace CustomerApi;

public record Customer(int Id, string Name) : IHasCustomerName
{
  public bool IsDeleted { get; set; }
}