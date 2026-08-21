namespace CustomerApi;

public static class OrderEndpoints
{
  const string ORDERS_ROUTE = "/orders";

  public static void MapOrderEndpoints(this WebApplication app)
  {
    app.MapGet(ORDERS_ROUTE, (int customerId, IOrderRepository repo) =>
    {
      List<Order> orders = repo.GetOrderByCustomerId(customerId);

      return orders;
    });

    app.MapGet(ORDERS_ROUTE + "/{id:int}", (int id, IOrderRepository repo) =>
    {
      var order = repo.GetOrderByOrderId(id);

      return order is not null ? Results.Ok(order) : Results.NotFound();
    });

    app.MapPost(ORDERS_ROUTE, (CreateOrderRequest request, IOrderRepository orderRepo, ICustomerRepository customerRepo) =>
    {
      var customer = customerRepo.GetCustomerById(request.CustomerId);
      if (customer is null)
      {
        return Results.NotFound();
      }

      var maxId = orderRepo.GetMaxId();
      var orderId = maxId + 1;
      var newOrder = new Order(orderId, request.CustomerId, request.Amount);
      orderRepo.Add(newOrder);

      return Results.Created($"{ORDERS_ROUTE}/{newOrder.Id}", newOrder);
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