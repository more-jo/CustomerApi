namespace CustomerApi;

using CustomerApi;

/// <summary>
/// Make the order not part of the customer, but its own object because of SRP. Also it is more difficult to traverse a predefined hierarchy /customers/id/order. 
/// </summary>
public record Order(int Id, int CustomerId, int Amount)
{
  public bool IsDeleted { get; set; }
}