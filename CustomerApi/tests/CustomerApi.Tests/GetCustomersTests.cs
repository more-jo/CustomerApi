using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace CustomerApi.Tests;

public class GetCustomersTests
{
    private async Task<HttpClient> CreateClient()
    {
        // this cannot be used for database approach : factory gets discarded. Database is created anew (empty/seedless) when called.
        var factory = new WebApplicationFactory<Program>();
        return factory.CreateClient();
    }

    [Test]
    public async Task SeedDataBase_DatabaseContainsSeedData()
    {
        // Arrange
        var factory = new WebApplicationFactory<Program>();
        factory.CreateClient();

        // Act
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CustomerDbContext>();
        var customers = db.Customers.ToList();

        // Assert
        Assert.That(customers, Is.Not.Empty);
    }

    [Test]
    public async Task GetCustomers_ReturnsCorrectArray()
    {
        // Arrange
        var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/customers");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var content = await response.Content.ReadAsStringAsync();
        // asp.net deserializes usually with camelCase, but Customer is PascalCase
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
    public async Task HappyPath_GetCustomerId_ReturnsCorrectCustomer()
    {
        // Arrange
        var client = await CreateClient();

        // Act
        var response = await client.GetAsync("/customers/1");

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
        // Arrange
        var client = await CreateClient();

        // Act
        var response = await client.GetAsync("/customers/999");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetCustomerById_NonExistentString_Returns404()
    {
        // Arrange
        var client = await CreateClient();

        // Act
        var response = await client.GetAsync("/customers/abc");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetCustomerEmpty_Returns400()
    {
        // Arrange
        var client = await CreateClient();

        // Act
        var response = await client.GetAsync(string.Empty);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
}