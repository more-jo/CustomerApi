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

    [SetUp]
    public async Task Setup()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
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
        var customer1 = new { Name = "Alice" };
        var responsePostCustomer1 = await _client.PostAsJsonAsync("/customers", customer1);
        var responsePostCustomerObject = await GetCustomerFromResponse(responsePostCustomer1);
        Assume.That(responsePostCustomerObject, Is.Not.Null);

        var customer2 = new { Name = "Bob" };
        var responsePostCustomer2 = await _client.PostAsJsonAsync("/customers", customer2);
        var responsePostCustomerObject2 = await GetCustomerFromResponse(responsePostCustomer2);
        Assume.That(responsePostCustomerObject2, Is.Not.Null);

        // Act
        var response = await _client.GetAsync("/customers");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var content = await response.Content.ReadAsStringAsync();
        // asp.net deserializes usually with camelCase, but Customer is PascalCase. Therefore options:
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var customers = JsonSerializer.Deserialize<List<Customer>>(content, options);
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
        var customer1 = new { Name = "Alice" };
        var responsePostCustomer1 = await _client.PostAsJsonAsync("/customers", customer1);
        var responsePostCustomerObject = await GetCustomerFromResponse(responsePostCustomer1);
        Assume.That(responsePostCustomerObject, Is.Not.Null);

        // Act
        var response = await _client.GetAsync($"/customers/{responsePostCustomerObject.Id}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var responseString = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var customer = JsonSerializer.Deserialize<Customer>(responseString, options);
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
        var newCustomer1 = new CreateCustomerRequest("1");
        var newCustomer2 = new CreateCustomerRequest("2");
        var newCustomer3 = new CreateCustomerRequest("3");

        var createdCustomer1Response = await _client.PostAsJsonAsync($"/customers", newCustomer1);
        Assume.That(createdCustomer1Response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var createdCustomer2Response = await _client.PostAsJsonAsync($"/customers", newCustomer2);
        Assume.That(createdCustomer2Response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var createdCustomer3Response = await _client.PostAsJsonAsync($"/customers", newCustomer3);
        Assume.That(createdCustomer3Response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        var createdCustomer2FromResponse = await GetCustomerFromContentAsync(createdCustomer2Response.Content);
        Assert.That(createdCustomer2FromResponse, Is.Not.Null);
        var deleteCustomerResponse = await _client.DeleteAsync($"/customers/{createdCustomer2FromResponse.Id}");
        Assume.That(deleteCustomerResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        // Act
        var response = await _client.GetAsync("/customers");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var content = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var customers = JsonSerializer.Deserialize<List<Customer>>(content, options);
        Assert.That(customers, Is.Not.Null);

        var deletedCustomer2 = customers.FirstOrDefault(c => c.Name == newCustomer2.Name);
        Assert.That(deletedCustomer2, Is.Not.Null);
        Assert.That(deletedCustomer2.IsDeleted, Is.True);

        var notDeletedCustomer1 = customers.FirstOrDefault(c => c.Name == newCustomer1.Name);
        Assert.That(notDeletedCustomer1, Is.Not.Null);
        Assert.That(notDeletedCustomer1.IsDeleted, Is.False);

        var notDeletedCustomer3 = customers.FirstOrDefault(c => c.Name == newCustomer3.Name);
        Assert.That(notDeletedCustomer3, Is.Not.Null);
        Assert.That(notDeletedCustomer3.IsDeleted, Is.False);
    }

    private async Task<CustomerResponse?> GetCustomerFromContentAsync(HttpContent content)
    {
        var responseString = await content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return JsonSerializer.Deserialize<CustomerResponse>(responseString, options);
    }

    private static async Task<Customer?> GetCustomerFromResponse(HttpResponseMessage response)
    {
        string responseJson = await response.Content.ReadAsStringAsync();
        JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };
        var customer = JsonSerializer.Deserialize<Customer>(responseJson, options);
        return customer;
    }
}