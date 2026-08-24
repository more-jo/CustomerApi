namespace CustomerApi;

public record CustomerResponse(int Id, string Name) : ICustomer
{
  public bool IsDeleted { get; set; }
}