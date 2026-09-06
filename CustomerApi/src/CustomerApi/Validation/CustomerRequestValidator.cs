
namespace CustomerApi;

public static class CustomerRequestValidator
{
  public static IResult? ValidateCustomerName(string? name)
  {
    if (string.IsNullOrWhiteSpace(name))
    {
      return Results.Problem(
          title: "Invalid request body",
          detail: "The request body is invalid or missing.",
          statusCode: (int)System.Net.HttpStatusCode.UnprocessableContent,
          extensions: new Dictionary<string, object?>
          {
            ["errorCode"] = "NAME_REQUIRED"
          }
      );
    }

    return null;
  }

  public static IResult? ValidateCustomerName(IHasCustomerName? customer)
  {
    if (customer is null)
    {
      return Results.BadRequest();
    }

    return ValidateCustomerName(customer.Name);
  }
}