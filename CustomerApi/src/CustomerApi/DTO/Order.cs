/// <summary>
/// Make the order not part of the customer, but its own object because of SRP. Also it is mord difficult to traverse a predefined chierarchy /customers/id/order. 
/// </summary>
/// <param name="Id"></param>
/// <param name="customerId"></param>
/// <param name="amount"></param>
public record Order(int Id, int CustomerId, int Amount)
{
  public bool IsDeleted { get; set; }
}