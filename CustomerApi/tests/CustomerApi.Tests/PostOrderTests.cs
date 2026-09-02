using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json;
using System.Net.Http.Json;

namespace CustomerApi.Tests;

public class PostOrderTests
{
  private WebApplicationFactory<Program> _factory = null!;
  private System.Net.Http.HttpClient _client = null!;
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
  public async Task PostOrder_CustomerExists_Returns201WithLocationHeader()
  {
    // Arrange
    var atLeastOneCustomer = await _testContextManager.CreateCustomerAsync(_client, "Alice");

    var order = new { CustomerId = atLeastOneCustomer.Id, Amount = 1 };

    // Act
    var responseOrderPost = await _client.PostAsJsonAsync("/orders", order);

    // Assert
    Assert.That(responseOrderPost.StatusCode, Is.EqualTo(HttpStatusCode.Created));

    var content = await responseOrderPost.Content.ReadAsStringAsync();
    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    var responseOrder = JsonSerializer.Deserialize<OrderResponse>(content, options);
    Assert.That(responseOrder, Is.Not.Null);

    Assert.That(responseOrderPost.Headers.Location?.ToString(), Is.EqualTo($"/orders/{responseOrder.Id}"));
    Assert.That(responseOrder.CustomerId, Is.EqualTo(order.CustomerId));
    Assert.That(responseOrder.Amount, Is.EqualTo(order.Amount));
  }

  [Test]
  public async Task PostOrder_CustomerDoesNotExist_Returns404()
  {
    // Arrange
    var nonExistentCustomer = int.MaxValue;
    var errorProvokingOrder = new { CustomerId = nonExistentCustomer, Amount = 1 };

    // Act
    var response = await _client.PostAsJsonAsync("/orders", errorProvokingOrder);

    // Assert
    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
  }
}