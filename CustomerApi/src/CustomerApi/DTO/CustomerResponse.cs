namespace CustomerApi;

public record CustomerResponse(int Id, string Name) : CustomerName(Name)
{
  public bool IsDeleted { get; set; }
}