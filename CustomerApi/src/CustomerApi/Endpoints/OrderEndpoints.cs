namespace CustomerApi;

public static class OrderEndpoints
{
  const string ORDERS_ROUTE = "/orders";
  const int UNASSIGNED_ID = 0;

  public static void MapOrderEndpoints(this WebApplication app)
  {
    app.MapGet(ORDERS_ROUTE, (int customerId, IOrderRepository repo) =>
    {
      List<Order> orders = repo.GetOrderByCustomerId(customerId);
      var orderResponses = orders.Select(o => OrderResponse.From(o));
      return orderResponses;
    });

    app.MapGet(ORDERS_ROUTE + "/{id:int}", (int id, IOrderRepository repo) =>
    {
      var order = repo.GetOrderByOrderId(id);

      return order is not null ? Results.Ok(OrderResponse.From(order)) : Results.NotFound();
    });

    app.MapPost(ORDERS_ROUTE, (CreateOrderRequest request, IOrderRepository orderRepo, ICustomerRepository customerRepo) =>
    {
      var customer = customerRepo.GetCustomerById(request.CustomerId);
      if (customer is null)
      {
        return Results.NotFound();
      }

      var newOrder = new Order(UNASSIGNED_ID, request.CustomerId, request.Amount);
      orderRepo.Add(newOrder);

      return Results.Created($"{ORDERS_ROUTE}/{newOrder.Id}", OrderResponse.From(newOrder));
    });

    app.MapDelete(ORDERS_ROUTE + "/{id:int}", (int id, IOrderRepository orderRepo) =>
    {
      if (orderRepo.Delete(id))
      {
        return Results.NoContent();
      }

      return Results.NotFound();
    });
  }
}