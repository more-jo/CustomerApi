namespace CustomerApi;

public static class CustomerEndPoints
{

  const string CUSTOMER_ROUTE = "/customers";

  public static void MapCustomerEndpoints(this WebApplication app)
  {
    app.MapGet(CUSTOMER_ROUTE, (ICustomerRepository repo) =>
    {
      List<Customer> customers = repo.GetAll();
      // Constructor injection: ASP.NET matches the parameter type (ICustomerRepository)
      // to the registered service and passes the instance here.
      return customers;
    });

    app.MapGet(CUSTOMER_ROUTE + "/{id:int}", (int id, ICustomerRepository repo) =>
    {
      var customer = repo.GetCustomerById(id);
      return customer is not null ? Results.Ok(customer) : Results.NotFound();
    });

    app.MapPost(CUSTOMER_ROUTE, (CreateCustomerRequest newCustomer, ICustomerRepository repo) =>
    {
      var validationResult = CustomerRequestValidator.ValidateCustomerName(newCustomer);
      if (validationResult is not null)
      {
        return validationResult;
      }

      int newId = repo.GetMaxId() + 1;
      var customer = new Customer(newId, newCustomer.Name);
      repo.Add(customer);

      return Results.Created($"{CUSTOMER_ROUTE}/{customer.Id}", customer);
    });

    app.MapPut(CUSTOMER_ROUTE + "/{id:int}", (int id, UpdateCustomerRequest newCustomer, ICustomerRepository repo) =>
    {
      var validationResult = CustomerRequestValidator.ValidateCustomerName(newCustomer);
      if (validationResult is not null)
      {
        return validationResult;
      }

      if (repo.Update(id, newCustomer.Name))
      {
        return Results.NoContent();
      }

      return Results.NotFound();
    });

    app.MapDelete(CUSTOMER_ROUTE + "/{id:int}", (int id, ICustomerRepository repo) =>
    {
      if (repo.Delete(id))
      {
        return Results.NoContent();
      }

      return Results.NotFound();
    });
  }
}