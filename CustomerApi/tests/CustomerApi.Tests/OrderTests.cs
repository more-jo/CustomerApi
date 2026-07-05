using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace CustomerApi.Tests;

public class OrderTests
{
  [Test]
  public async Task GetOrders_CustomerExistsWithNoOrders_ReturnsEmptyList()
  {
    // Arrange
    await using var factory = new WebApplicationFactory<Program>();
    var client = factory.CreateClient();

    var newCustomer = new { Name = "Alice" };
    string newCustomerJson = JsonSerializer.Serialize(newCustomer);
    var httpContent = new StringContent(newCustomerJson, System.Text.Encoding.UTF8, "application/json");

    var responsePost = await client.PostAsync("/customers", httpContent);

    // Act
    var response = await client.GetAsync("/orders?customerId=1");

    // Asssert
    Assert.That(responsePost.StatusCode, Is.EqualTo(HttpStatusCode.Created));

    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

    var content = await response.Content.ReadAsStringAsync();
    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    var orders = JsonSerializer.Deserialize<List<Order>>(content, options);
    Assert.That(orders, Is.Empty);
  }
}