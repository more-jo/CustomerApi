namespace CustomerApi;

/// <summary>
/// Repository abstraction for order data access.
/// </summary>
interface IOrderRepository
{
  List<Order> GetOrderByCustomerId(int customerId);

  Order? GetOrderByOrderId(int orderId);

  int GetMaxId();

  void Add(Order order);

  bool Delete(int id);
}