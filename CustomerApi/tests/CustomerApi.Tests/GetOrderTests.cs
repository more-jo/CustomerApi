using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json;
using System.Net.Http.Json;

namespace CustomerApi.Tests;

public class GetOrderTests
{
  private WebApplicationFactory<Program> _factory = null!;
  private System.Net.Http.HttpClient _client;
  private TestContextManager _testContextManager;

  [SetUp]
  public void Setup()
  {
    _factory = new WebApplicationFactory<Program>();
    _client = _factory.CreateClient();

    _testContextManager = new TestContextManager();
  }

  [TearDown]
  public async Task TearDown()
  {
    _client.Dispose();
    await _factory.DisposeAsync();
  }

  [Test]
  public async Task GetOrders_CustomerExistsWithNoOrders_ReturnsEmptyList()
  {
    // Arrange
    var newCustomer = await _testContextManager.CreateCustomerAsync(_client, "Alice");

    // Act
    var response = await _client.GetAsync($"/orders?customerId={newCustomer.Id}");

    // Asssert
    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

    var content = await response.Content.ReadAsStringAsync();
    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    var orders = JsonSerializer.Deserialize<List<Order>>(content, options);
    Assert.That(orders, Is.Empty);
  }


  [Test]
  public async Task GetOrder_CustomerExists_Returns200()
  {
    // Arrange 
    var newCustomer = await _testContextManager.CreateCustomerAsync(_client, "Alice");

    var expectedOrder = new { CustomerId = newCustomer.Id, Amount = 1 };
    var responsePostOrder = await _client.PostAsJsonAsync("/orders", expectedOrder);

    // Act 
    var responseGetOrder = await _client.GetAsync($"/orders?CustomerId={expectedOrder.CustomerId}");

    // Assert
    Assert.That(responsePostOrder.StatusCode, Is.EqualTo(HttpStatusCode.Created));
    Assert.That(responseGetOrder.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    var content = await responseGetOrder.Content.ReadAsStringAsync();
    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    var responseOrders = JsonSerializer.Deserialize<List<Order>>(content, options);
    Assert.That(responseOrders, Is.Not.Null);
    Assert.That(responseOrders.Where(o => o.Amount == 1 && o.CustomerId == expectedOrder.CustomerId).Count, Is.EqualTo(1));
  }

  [Test]
  public async Task GetOrder_OrderIdExists_Returns200()
  {
    // Arrange
    var newCustomer = await _testContextManager.CreateCustomerAsync(_client, "Alice");

    var createdOrder = await _testContextManager.CreateOrderAsync(_client, newCustomer.Id, 1);

    // Act
    var responseGetOrder = await _client.GetAsync($"/orders/{createdOrder.Id}");

    // Assert
    Assert.That(responseGetOrder.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    Assert.That(responseGetOrder.Content, Is.Not.Null);

    var responseOrder = await _testContextManager.GetOrderFromHttpResponse(responseGetOrder);
    Assert.That(responseOrder, Is.Not.Null);
    Assert.That(responseOrder.Amount, Is.EqualTo(1));
    Assert.That(responseOrder.Id, Is.EqualTo(createdOrder.Id));
    Assert.That(responseOrder.CustomerId, Is.EqualTo(newCustomer.Id));
  }

  [Test]
  public async Task GetOrder_OrderIdAbsent_Returns404()
  {
    // Act
    var responseGetOrder = await _client.GetAsync($"/orders/999");

    // Assert
    Assert.That(responseGetOrder.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
  }
}