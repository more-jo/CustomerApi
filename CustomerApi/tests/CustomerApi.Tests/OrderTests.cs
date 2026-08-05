using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Unicode;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using System.Runtime.CompilerServices;

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
    var errorProvokingOrder = new { CustomerId = nonExistentCustomer, Amount = 1 };

    // Act
    var response = await client.PostAsJsonAsync("/orders", errorProvokingOrder);

    // Assert
    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
  }

  [Test]
  public async Task GetOrder_CustomerExists_Returns200()
  {
    // Arrange 
    await using var factory = new WebApplicationFactory<Program>();
    var client = factory.CreateClient();
    var atLeastOneCustomer = new { Name = "Alice" };
    var responsePostCustomer = await client.PostAsJsonAsync("/customers", atLeastOneCustomer);
    var expectedOrder = new { CustomerId = 1, Amount = 1 };
    var responsePostOrder = await client.PostAsJsonAsync("/orders", expectedOrder);

    // Act 
    var responseGetOrder = await client.GetAsync($"/orders?CustomerId={expectedOrder.CustomerId}");

    // Assert
    Assert.That(responsePostCustomer.StatusCode, Is.EqualTo(HttpStatusCode.Created));
    Assert.That(responsePostOrder.StatusCode, Is.EqualTo(HttpStatusCode.Created));
    Assert.That(responseGetOrder.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    var content = await responseGetOrder.Content.ReadAsStringAsync();
    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    var responseOrders = JsonSerializer.Deserialize<List<Order>>(content, options);
    Assert.That(responseOrders, Is.Not.Null);
    Assert.That(responseOrders.Where(o => o.Amount == 1 && o.CustomerId == 1).Count, Is.EqualTo(1));
  }

  [Test]
  public async Task GetOrder_OrderExists_Returns200()
  {
    // Arrange
    await using var factory = new WebApplicationFactory<Program>();
    var client = factory.CreateClient();
    var atLeastOneCustomer = new { Name = "Alice" };
    var responsePostCustomer = await client.PostAsJsonAsync("/customers", atLeastOneCustomer);
    Assert.That(responsePostCustomer.StatusCode, Is.EqualTo(HttpStatusCode.Created));

    var expectedOrder = new { Amount = 1, CustomerId = 1 };
    var responsePostOrder = await client.PostAsJsonAsync("/orders", expectedOrder);
    Assert.That(responsePostOrder.StatusCode, Is.EqualTo(HttpStatusCode.Created));

    var createdOrderContent = await responsePostOrder.Content.ReadAsStringAsync();
    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    var createdOrder = JsonSerializer.Deserialize<Order>(createdOrderContent, options);

    // Act
    var responseGetOrder = await client.GetAsync($"/orders/{createdOrder.Id}");

    // Assert
    Assert.That(responseGetOrder.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    Assert.That(responseGetOrder.Content, Is.Not.Null);
    var content = await responseGetOrder.Content.ReadAsStringAsync();
    var responseOrder = JsonSerializer.Deserialize<Order>(content, options);
    Assert.That(responseOrder, Is.Not.Null);
    Assert.That(responseOrder.Amount, Is.EqualTo(1));
    Assert.That(responseOrder.Id, Is.EqualTo(createdOrder.Id));
    Assert.That(responseOrder.CustomerId, Is.EqualTo(1));
  }
}