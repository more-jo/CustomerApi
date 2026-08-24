using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;

namespace CustomerApi.Tests;

public class GetCustomersTests
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;
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
    public async Task GetCustomers_ReturnsCorrectArray()
    {
        // Arrange
        await _testContextManager.CreateCustomerAsync(_client, "Alice");
        await _testContextManager.CreateCustomerAsync(_client, "Bob");

        // Act
        var response = await _client.GetAsync("/customers");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var content = await response.Content.ReadAsStringAsync();
        // asp.net deserializes usually with camelCase, but Customer is PascalCase. Therefore options:
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var customers = JsonSerializer.Deserialize<List<CustomerResponse>>(content, options);
        Assert.That(customers, Is.Not.Null);
        Assert.That(customers.Count, Is.EqualTo(2));
        Assert.That(customers[0].Name, Is.EqualTo("Alice"));
        Assert.That(customers[0].Id, Is.EqualTo(1));
        Assert.That(customers[1].Name, Is.EqualTo("Bob"));
        Assert.That(customers[1].Id, Is.EqualTo(2));
    }

    [Test]
    public async Task HappyPath_GetCustomerId_ReturnsCorrectCustomer200()
    {
        // Arrange
        var responsePostCustomerObject = await _testContextManager.CreateCustomerAsync(_client, "Alice");

        // Act
        var response = await _client.GetAsync($"/customers/{responsePostCustomerObject.Id}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var responseString = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var customer = JsonSerializer.Deserialize<CustomerResponse>(responseString, options);
        Assert.That(customer, Is.Not.Null);
        Assert.That(customer.Name, Is.EqualTo("Alice"));
    }

    [Test]
    public async Task GetCustomerById_NonExistentId_Returns404()
    {
        // Act
        var response = await _client.GetAsync("/customers/999");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetCustomerById_NonExistentString_Returns404()
    {
        // Act
        var response = await _client.GetAsync("/customers/abc");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetCustomerEmptyRoute_Returns404()
    {
        // Act
        var response = await _client.GetAsync(string.Empty);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetCustomers_DeleteOne_ReturnsCustomersWithCorrectDeletionFlag()
    {
        // Arrange
        var newCustomerName1 = "1";
        var newCustomerName2 = "2";
        var newCustomerName3 = "3";
        await _testContextManager.CreateCustomerAsync(_client, newCustomerName1);
        var createdCustomer2FromResponse = await _testContextManager.CreateCustomerAsync(_client, newCustomerName2);
        await _testContextManager.CreateCustomerAsync(_client, newCustomerName3);

        var deleteCustomerResponse = await _client.DeleteAsync($"/customers/{createdCustomer2FromResponse.Id}");
        Assume.That(deleteCustomerResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        // Act
        var response = await _client.GetAsync("/customers");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var content = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var customers = JsonSerializer.Deserialize<List<CustomerResponse>>(content, options);
        Assert.That(customers, Is.Not.Null);

        var deletedCustomer2 = customers.FirstOrDefault(c => c.Name == newCustomerName2);
        Assert.That(deletedCustomer2, Is.Not.Null);
        Assert.That(deletedCustomer2.IsDeleted, Is.True);

        var notDeletedCustomer1 = customers.FirstOrDefault(c => c.Name == newCustomerName1);
        Assert.That(notDeletedCustomer1, Is.Not.Null);
        Assert.That(notDeletedCustomer1.IsDeleted, Is.False);

        var notDeletedCustomer3 = customers.FirstOrDefault(c => c.Name == newCustomerName3);
        Assert.That(notDeletedCustomer3, Is.Not.Null);
        Assert.That(notDeletedCustomer3.IsDeleted, Is.False);
    }
}