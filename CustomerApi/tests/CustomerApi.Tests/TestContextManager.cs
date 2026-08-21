
namespace CustomerApi.Tests;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

public class TestContextManager
{
  public async Task<Customer> CreateCustomerAsync(HttpClient client, string name)
  {
    var newCustomer = new { Name = name };
    var responsePost = await client.PostAsJsonAsync("/customers/", newCustomer);
    Assume.That(responsePost.StatusCode, Is.EqualTo(HttpStatusCode.Created));

    var responseCustomer = await GetCustomerFromResponse(responsePost);
    Assume.That(responseCustomer, Is.Not.Null);

    return responseCustomer;
  }

  // public static async Task<Order> CreateOrderAsync(HttpClient client, int customerId, int amount)
  // {

  // }

  private static async Task<Customer?> GetCustomerFromResponse(HttpResponseMessage response)
  {
    string responseJson = await response.Content.ReadAsStringAsync();
    JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };
    var customer = JsonSerializer.Deserialize<Customer>(responseJson, options);
    return customer;
  }
}