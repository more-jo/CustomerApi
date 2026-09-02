namespace CustomerApi;

public static class CustomerEndPoints
{
  const string CUSTOMER_ROUTE = "/customers";

  public static void MapCustomerEndpoints(this WebApplication app)
  {
    app.MapGet(CUSTOMER_ROUTE, (ICustomerRepository repo) =>
    {
      List<Customer> customers = repo.GetAll();
      var customerResponseList = customers.Select(c => CustomerResponse.From(c));
      return customerResponseList;
    });

    app.MapGet(CUSTOMER_ROUTE + "/{id:int}", (int id, ICustomerRepository repo) =>
    {
      var customer = repo.GetCustomerById(id);
      return customer is not null ? Results.Ok(CustomerResponse.From(customer)) : Results.NotFound();
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

    app.MapPatch(CUSTOMER_ROUTE + "/{id:int}", (int id, PatchCustomerRequest patchRequest, ICustomerRepository repo) =>
    {
      var foundCustomer = repo.GetCustomerById(id);
      if (foundCustomer is null)
      {
        return Results.NotFound();
      }

      Customer patchedCustomer = foundCustomer;
      if (!string.IsNullOrEmpty(patchRequest.Name))
      {
        var validationResult = CustomerRequestValidator.ValidateCustomerName(patchRequest.Name);
        if (validationResult is not null)
        {
          return validationResult;
        }
        patchedCustomer = foundCustomer with { Name = patchRequest.Name };
      }

      if (patchRequest.IsDeleted.HasValue)
      {
        patchedCustomer = patchedCustomer with { IsDeleted = patchRequest.IsDeleted.Value };
      }

      var checkResult = repo.Patch(patchedCustomer);
      if (checkResult.IsSuccess)
      {
        return Results.NoContent();
      }

      return Results.NotFound();
    });
  }
}