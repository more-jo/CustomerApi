namespace CustomerApi;

public record OrderResponse(int Id, int CustomerId, int Amount, bool IsDeleted)
{
  public static OrderResponse From(Order order) =>
      new(order.Id, order.CustomerId, order.Amount, order.IsDeleted);
}