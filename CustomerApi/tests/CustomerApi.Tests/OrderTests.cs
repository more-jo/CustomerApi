using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json;
using System.Net.Http.Json;

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
    var responsePostCustomerObject = await GetCustomerFromResponse(responsePost);

    // Act
    var response = await client.GetAsync($"/orders?customerId={responsePostCustomerObject.Id}");

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

    var atLeastOneCustomer = new { Name = "Alice" };
    var responseCustomerPost = await client.PostAsJsonAsync("/customers", atLeastOneCustomer);
    var responseCustomerPostObject = await GetCustomerFromResponse(responseCustomerPost);

    var order = new { CustomerId = responseCustomerPostObject.Id, Amount = 1 };

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
    var responsePostCustomerObject = await GetCustomerFromResponse(responsePostCustomer);

    var expectedOrder = new { CustomerId = responsePostCustomerObject.Id, Amount = 1 };
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
  public async Task GetOrder_OrderIdExists_Returns200()
  {
    // Arrange
    await using var factory = new WebApplicationFactory<Program>();
    var client = factory.CreateClient();
    var atLeastOneCustomer = new { Name = "Alice" };
    var responsePostCustomer = await client.PostAsJsonAsync("/customers", atLeastOneCustomer);
    Assume.That(responsePostCustomer.StatusCode, Is.EqualTo(HttpStatusCode.Created));

    var responsePostCustomerObject = await GetCustomerFromResponse(responsePostCustomer);

    var expectedOrder = new { Amount = 1, CustomerId = responsePostCustomerObject.Id };
    var responsePostOrder = await client.PostAsJsonAsync("/orders", expectedOrder);
    Assume.That(responsePostOrder.StatusCode, Is.EqualTo(HttpStatusCode.Created));

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

  [Test]
  public async Task GetOrder_OrderIdAbsent_Returns404()
  {
    // Arrange
    await using var factory = new WebApplicationFactory<Program>();
    var client = factory.CreateClient();

    // Act
    var responseGetOrder = await client.GetAsync($"/orders/999");

    // Assert
    Assert.That(responseGetOrder.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
  }

  [Test]
  public async Task Delete_ExistingOrder_Returns204()
  {
    // Arrange
    await using var factory = new WebApplicationFactory<Program>();
    var client = factory.CreateClient();

    var atLeastOneCustomer = new { Name = "Charlie" };
    var atLeastOneCustomerResponse = await client.PostAsJsonAsync("/customers", atLeastOneCustomer);
    Assume.That(atLeastOneCustomerResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
    var createdCustomer = await GetCustomerFromResponse(atLeastOneCustomerResponse);
    Assume.That(createdCustomer, Is.Not.Null);

    var atLeastOneOrder = new { Amount = 1, CustomerId = createdCustomer.Id };
    var responsePostOrder = await client.PostAsJsonAsync("/orders", atLeastOneOrder);
    Assume.That(responsePostOrder.StatusCode, Is.EqualTo(HttpStatusCode.Created));

    Order? createdOrder = await GetOrderFromHttpResponse(responsePostOrder);
    Assume.That(createdOrder, Is.Not.Null);
    Assume.That(createdOrder.IsDeleted, Is.False);

    // Act
    var deleteOrderResponse = await client.DeleteAsync($"/orders/{createdOrder.Id}");

    // Assert
    Assert.That(deleteOrderResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    var getDeletedOrder = await client.GetAsync($"/orders/{createdOrder.Id}");
    var deletedOrder = await GetOrderFromHttpResponse(getDeletedOrder);
    Assert.That(deletedOrder, Is.Not.Null);
    Assert.That(deletedOrder.IsDeleted, Is.True);
  }

  [Test]
  public async Task DeleteCustomer_OrderOfDeletedUser_IsNotDeleted()
  {
    // Arrange
    await using var factory = new WebApplicationFactory<Program>();
    var client = factory.CreateClient();

    var atLeastOneCustomer = new { Name = "Charlie" };
    var atLeastOneCustomerResponse = await client.PostAsJsonAsync("/customers", atLeastOneCustomer);
    Assume.That(atLeastOneCustomerResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
    var createdCustomer = await GetCustomerFromResponse(atLeastOneCustomerResponse);
    Assume.That(createdCustomer, Is.Not.Null);

    var atLeastOneOrder = new CreateOrderRequest(createdCustomer.Id, 1);
    var atLeastOneOrderResponse = await client.PostAsJsonAsync("/orders", atLeastOneOrder);
    Assume.That(atLeastOneOrderResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
    var atLeastOneOrderResponseObject = await GetOrderFromHttpResponse(atLeastOneOrderResponse);
    Assume.That(atLeastOneOrderResponseObject, Is.Not.Null);
    Assume.That(atLeastOneOrderResponseObject.IsDeleted, Is.False);

    var deleteCustomerResponse = await client.DeleteAsync($"/customers/{createdCustomer.Id}");
    Assume.That(deleteCustomerResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

    // Act
    var getDeletedOrderResponse = await client.GetAsync($"/orders/{atLeastOneOrderResponseObject.Id}");

    // Assert
    var deletedOrder = await GetOrderFromHttpResponse(getDeletedOrderResponse);
    Assert.That(deletedOrder, Is.Not.Null);
    Assert.That(deletedOrder.IsDeleted, Is.False);
  }

  [Test]
  public async Task GetOrdersByCustomerId_OneOrderDeleted_ReturnsAllOrdersWithCorrectFlags()
  {
    // Arrange
    await using var factory = new WebApplicationFactory<Program>();
    var client = factory.CreateClient();

    var atLeastOneCustomer = new { Name = "Charlie" };
    var atLeastOneCustomerResponse = await client.PostAsJsonAsync("/customers", atLeastOneCustomer);
    Assume.That(atLeastOneCustomerResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
    var createdCustomer = await GetCustomerFromResponse(atLeastOneCustomerResponse);
    Assume.That(createdCustomer, Is.Not.Null);

    var atLeastOneOrder = new { Amount = 1, CustomerId = createdCustomer.Id };
    var responsePostOrder = await client.PostAsJsonAsync("/orders", atLeastOneOrder);
    Assume.That(responsePostOrder.StatusCode, Is.EqualTo(HttpStatusCode.Created));

    Order? createdOrder = await GetOrderFromHttpResponse(responsePostOrder);
    Assume.That(createdOrder, Is.Not.Null);
    Assume.That(createdOrder.IsDeleted, Is.False);

    var order2 = new { Amount = 1, CustomerId = createdCustomer.Id };
    var responsePostOrder2 = await client.PostAsJsonAsync("/orders", order2);
    Assume.That(responsePostOrder2.StatusCode, Is.EqualTo(HttpStatusCode.Created));

    Order? createdOrder2 = await GetOrderFromHttpResponse(responsePostOrder2);
    Assume.That(createdOrder2, Is.Not.Null);
    Assume.That(createdOrder2.IsDeleted, Is.False);

    var deleteOrderResponse = await client.DeleteAsync($"/orders/{createdOrder.Id}");
    Assume.That(deleteOrderResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

    // Act
    var getOrderResponse = await client.GetAsync($"/orders?customerId={createdCustomer.Id}");

    // Assert
    Assert.That(getOrderResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    var orders = await GetOrdersFromHttpResponse(getOrderResponse);
    Assert.That(orders, Is.Not.Null);

    var deletedOrder = orders.FirstOrDefault(o => o.Id == createdOrder.Id);
    Assert.That(deletedOrder, Is.Not.Null);
    Assert.That(deletedOrder.IsDeleted, Is.True);

    var untouchedOrder = orders.FirstOrDefault(o => o.Id == createdOrder2.Id);
    Assert.That(untouchedOrder, Is.Not.Null);
    Assert.That(untouchedOrder.IsDeleted, Is.False);
  }

  private static async Task<Customer> GetCustomerFromResponse(HttpResponseMessage response)
  {
    string responseJson = await response.Content.ReadAsStringAsync();
    JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };
    var customer = JsonSerializer.Deserialize<Customer>(responseJson, options);
    return customer;
  }

  private static async Task<Order?> GetOrderFromHttpResponse(HttpResponseMessage responsePostOrder)
  {
    var createdOrderContent = await responsePostOrder.Content.ReadAsStringAsync();
    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    var createdOrder = JsonSerializer.Deserialize<Order>(createdOrderContent, options);
    return createdOrder;
  }

  private static async Task<List<Order>?> GetOrdersFromHttpResponse(HttpResponseMessage response)
  {
    var createdOrderContent = await response.Content.ReadAsStringAsync();
    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    var createdOrder = JsonSerializer.Deserialize<List<Order>>(createdOrderContent, options);
    return createdOrder;
  }
}