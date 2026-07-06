using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Unicode;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;

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

  [Test]
  public async Task PostOrder_CustomerExists_Returns201WithLocationHeader()
  {
    // Arrange
    await using var factory = new WebApplicationFactory<Program>();
    var client = factory.CreateClient();
    const int expectedCustomerIdFromSeeding = 1;

    var atLeastOneCustomer = new { Name = "Alice" };
    var responseCustomerPost = await client.PostAsJsonAsync("/customers", atLeastOneCustomer);

    var order = new { CustomerId = 1, Amount = 1 };
    // Act
    var responseOrderPost = await client.PostAsJsonAsync("/orders", order);

    // Assert
    Assert.That(responseCustomerPost.StatusCode, Is.EqualTo(HttpStatusCode.Created));

    Assert.That(responseOrderPost.StatusCode, Is.EqualTo(HttpStatusCode.Created));
    var content = await responseOrderPost.Content.ReadAsStringAsync();
    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    var responseOrder = JsonSerializer.Deserialize<Order>(content, options);
    Assert.That(responseOrder, Is.Not.Null);
    Assert.That(responseOrderPost.Headers.Location?.ToString(), Is.EqualTo($"/orders/{responseOrder.Id}"));
    Assert.That(responseOrder.CustomerId, Is.EqualTo(order.CustomerId));
    Assert.That(responseOrder.Amount, Is.EqualTo(order.Amount));
    Assert.That(responseOrder.Id, Is.EqualTo(expectedCustomerIdFromSeeding));
  }

  [Test]
  public async Task PostOrder_CustomerDoesNotExist_Returns404()
  {
    // Arrange
    await using var factory = new WebApplicationFactory<Program>();
    var client = factory.CreateClient();
    var nonExistentCustomer = int.MaxValue;
    var errorProvokingOrder = new { customerID = nonExistentCustomer, Amount = 1 };

    // Act
    var response = await client.PostAsJsonAsync("/orders", errorProvokingOrder);

    // Assert
    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
  }
}