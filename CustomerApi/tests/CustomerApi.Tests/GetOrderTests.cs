using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json;
using System.Net.Http.Json;

namespace CustomerApi.Tests;

public class OrderTests
{
  private WebApplicationFactory<Program> _factory = null!;
  private System.Net.Http.HttpClient _client;
  private TestContextManager _testContextManager;

  [SetUp]
  public async Task Setup()
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

  [Test]
  public async Task Delete_ExistingOrder_Returns204()
  {
    // Arrange
    var newCustomer = await _testContextManager.CreateCustomerAsync(_client, "Alice");

    var atLeastOneOrder = await _testContextManager.CreateOrderAsync(_client, newCustomer.Id, 1);

    // Act
    var deleteOrderResponse = await _client.DeleteAsync($"/orders/{atLeastOneOrder.Id}");

    // Assert
    Assert.That(deleteOrderResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

    var getDeletedOrder = await _client.GetAsync($"/orders/{atLeastOneOrder.Id}");
    var deletedOrder = await _testContextManager.GetOrderFromHttpResponse(getDeletedOrder);
    Assert.That(deletedOrder, Is.Not.Null);
    Assert.That(deletedOrder.IsDeleted, Is.True);
  }

  [Test]
  public async Task DeleteCustomer_OrderOfDeletedUser_IsNotDeleted()
  {
    // Arrange
    var atLeastOneCustomer = await _testContextManager.CreateCustomerAsync(_client, "Alice");

    await _testContextManager.CreateOrderAsync(_client, atLeastOneCustomer.Id, 1);

    var deleteCustomerResponse = await _client.DeleteAsync($"/customers/{atLeastOneCustomer.Id}");
    Assume.That(deleteCustomerResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

    // Act
    var getDeletedOrderResponse = await _client.GetAsync($"/orders/{atLeastOneCustomer.Id}");

    // Assert
    var deletedOrder = await _testContextManager.GetOrderFromHttpResponse(getDeletedOrderResponse);
    Assert.That(deletedOrder, Is.Not.Null);
    Assert.That(deletedOrder.IsDeleted, Is.False);
  }

  [Test]
  public async Task GetOrdersByCustomerId_OneOrderDeleted_ReturnsAllOrdersWithCorrectFlags()
  {
    // Arrange
    var atLeastOneCustomer = await _testContextManager.CreateCustomerAsync(_client, "Charlie");

    var order1 = await _testContextManager.CreateOrderAsync(_client, atLeastOneCustomer.Id, 1);
    Assume.That(order1.IsDeleted, Is.False);

    var order2 = await _testContextManager.CreateOrderAsync(_client, atLeastOneCustomer.Id, 1);
    Assume.That(order2.IsDeleted, Is.False);

    var deleteOrderResponse = await _client.DeleteAsync($"/orders/{order1.Id}");
    Assume.That(deleteOrderResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

    // Act
    var getOrderResponse = await _client.GetAsync($"/orders?customerId={atLeastOneCustomer.Id}");

    // Assert
    Assert.That(getOrderResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    var orders = await _testContextManager.GetOrdersFromHttpResponse(getOrderResponse);
    Assert.That(orders, Is.Not.Null);

    var deletedOrder = orders.FirstOrDefault(o => o.Id == order1.Id);
    Assert.That(deletedOrder, Is.Not.Null);
    Assert.That(deletedOrder.IsDeleted, Is.True);

    var untouchedOrder = orders.FirstOrDefault(o => o.Id == order2.Id);
    Assert.That(untouchedOrder, Is.Not.Null);
    Assert.That(untouchedOrder.IsDeleted, Is.False);
  }
}