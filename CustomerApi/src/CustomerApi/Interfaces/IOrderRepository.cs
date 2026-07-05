namespace CustomerApi;

/// <summary>
/// Repository abstraction for order data access.
/// </summary>
interface IOrderRepository
{
  List<Order> GetOrderByCustomerId(int customerId);
}