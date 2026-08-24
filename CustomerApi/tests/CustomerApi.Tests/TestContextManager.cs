
namespace CustomerApi.Tests;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

public class TestContextManager
{
  public async Task<CustomerResponse> CreateCustomerAsync(HttpClient client, string name)
  {
    var newCustomer = new { Name = name };
    var responsePost = await client.PostAsJsonAsync("/customers/", newCustomer);
    Assume.That(responsePost.StatusCode, Is.EqualTo(HttpStatusCode.Created));

    var responseCustomer = await GetCustomerFromResponse(responsePost);
    Assume.That(responseCustomer, Is.Not.Null);

    return responseCustomer;
  }

  private async Task<CustomerResponse?> GetCustomerFromResponse(HttpResponseMessage response)
  {
    string responseJson = await response.Content.ReadAsStringAsync();
    JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };
    var customer = JsonSerializer.Deserialize<CustomerResponse>(responseJson, options);
    return customer;
  }

  public async Task<Order> CreateOrderAsync(HttpClient client, int customerId, int amount)
  {
    var atLeastOneOrder = new { Amount = amount, CustomerId = customerId };
    var responsePostOrder = await client.PostAsJsonAsync("/orders", atLeastOneOrder);
    Assume.That(responsePostOrder.StatusCode, Is.EqualTo(HttpStatusCode.Created));

    Order? createdOrder = await GetOrderFromHttpResponse(responsePostOrder);
    Assume.That(createdOrder, Is.Not.Null);

    return createdOrder;
  }

  public async Task<Order?> GetOrderFromHttpResponse(HttpResponseMessage responsePostOrder)
  {
    var createdOrderContent = await responsePostOrder.Content.ReadAsStringAsync();
    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    var createdOrder = JsonSerializer.Deserialize<Order>(createdOrderContent, options);
    return createdOrder;
  }

  public async Task<List<Order>?> GetOrdersFromHttpResponse(HttpResponseMessage response)
  {
    var createdOrderContent = await response.Content.ReadAsStringAsync();
    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    var createdOrder = JsonSerializer.Deserialize<List<Order>>(createdOrderContent, options);
    return createdOrder;
  }
}