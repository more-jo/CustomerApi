namespace CustomerApi;

public record Order(int Id, int CustomerId, int Amount)
{
  public bool IsDeleted { get; set; }
}