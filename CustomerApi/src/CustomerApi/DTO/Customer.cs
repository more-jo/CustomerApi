namespace CustomerApi;

public record Customer(int Id, string Name)
{
  public bool IsDeleted { get; set; }
}