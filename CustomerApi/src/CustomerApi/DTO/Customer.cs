namespace CustomerApi;

public record Customer(int Id, string Name) : CustomerName(Name)
{
  public bool IsDeleted { get; set; }
}