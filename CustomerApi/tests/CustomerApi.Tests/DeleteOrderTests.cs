using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CustomerApi.Tests;

public class DeleteOrderTests
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
  public async Task Delete_ExistingOrder_Returns204()
  {
    // Arrange
    var atLeastOneCustomer = await _testContextManager.CreateCustomerAsync(_client, "Alice");
    var atLeastOneOrder = await _testContextManager.CreateOrderAsync(_client, atLeastOneCustomer.Id, 1);
    Assume.That(atLeastOneOrder.IsDeleted, Is.False);

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
    var atLeastOneOrder = await _testContextManager.CreateOrderAsync(_client, atLeastOneCustomer.Id, 1);

    Assume.That(atLeastOneOrder.IsDeleted, Is.False);

    var deleteCustomerResponse = await _client.DeleteAsync($"/customers/{atLeastOneCustomer.Id}");
    Assume.That(deleteCustomerResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

    // Act
    var getDeletedOrderResponse = await _client.GetAsync($"/orders/{atLeastOneOrder.Id}");

    // Assert
    var deletedOrder = await _testContextManager.GetOrderFromHttpResponse(getDeletedOrderResponse);
    Assert.That(deletedOrder, Is.Not.Null);
    Assert.That(deletedOrder.IsDeleted, Is.False);
  }

  [Test]
  public async Task GetOrdersByCustomerId_OneOrderDeleted_ReturnsAllOrdersWithCorrectFlags()
  {
    // Arrange
    var atLeastOneCustomer = await _testContextManager.CreateCustomerAsync(_client, "Alice");
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