using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json;

namespace CustomerApi.Tests;

public class PostCustomerTests
{
    [Test]
    public async Task PostCustomer_Returns201WithLocationHeader()
    {
        // Arrange
        await using var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();
        var newCustomer = new { Name = "Charlie" };
        string newCustomerJson = JsonSerializer.Serialize(newCustomer);
        var httpContent = new StringContent(newCustomerJson, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/customers", httpContent);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        Assert.That(response.Headers.Location, Is.Not.Null);
        Assert.That(response.Headers.Location, Is.EqualTo(new Uri("/customers/3", UriKind.Relative)));
        string responseJson = await response.Content.ReadAsStringAsync();
        JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };
        var createdCustomer = JsonSerializer.Deserialize<Customer>(responseJson, options);
        Assert.That(createdCustomer, Is.Not.Null);
        Assert.That(createdCustomer.Name, Is.EqualTo("Charlie"));
    }

    [Test]
    public async Task PostCustomer_ThenGetCustomers_ReturnsNewCustomerInList()
    {
        // Arrange
        await using var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();
        string newCustomerJson = JsonSerializer.Serialize(new { Name = "Charlie" });
        var httpContent = new StringContent(newCustomerJson, System.Text.Encoding.UTF8, "application/json");

        // Act
        var responsePost = await client.PostAsync("/customers", httpContent);

        var responseGet = await client.GetAsync("/customers");
        var content = await responseGet.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var customers = JsonSerializer.Deserialize<List<Customer>>(content, options);

        // Assert
        Assert.That(responsePost.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        Assert.That(responseGet.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(customers, Is.Not.Null);
        Assert.That(customers.Count, Is.EqualTo(3));
        Assert.That(customers[2].Name, Is.EqualTo("Charlie"));
    }

    [Test]
    public async Task PostCustomer_WhenNameIsEmpty_Returns422()
    {
        // Arrange
        await using var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();

        string newCustomerJson = JsonSerializer.Serialize(new { Name = string.Empty });
        var httpContent = new StringContent(newCustomerJson, System.Text.Encoding.UTF8, "application/json");

        // Act
        var responsePost = await client.PostAsync("/customers", httpContent);

        // Assert
        Assert.That(responsePost.StatusCode, Is.EqualTo(HttpStatusCode.UnprocessableEntity));
    }

    [Test]
    public async Task PostCustomer_WhenIsEmpty_Returns422()
    {
        // Arrange
        await using var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();

        var httpContent = new StringContent("{ }", System.Text.Encoding.UTF8, "application/json");

        // Act
        var responsePost = await client.PostAsync("/customers", httpContent);

        // Assert
        Assert.That(responsePost.StatusCode, Is.EqualTo(HttpStatusCode.UnprocessableEntity));
    }

    [Test]
    public async Task PostCustomer_WhenMalformedJson_Returns400()
    {
        // Arrange
        await using var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();

        var httpContent = new StringContent("{ Name: }", System.Text.Encoding.UTF8, "application/json");

        // Act
        var responsePost = await client.PostAsync("/customers", httpContent);

        // Assert
        Assert.That(responsePost.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task PostCustomer_WhenNameIsMissing_Returns422WithProblemDetails()
    {
        // Arrange
        await using var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();

        var httpContent = new StringContent("{ }", System.Text.Encoding.UTF8, "application/json");

        // Act
        var responsePost = await client.PostAsync("/customers", httpContent);

        var content = await responsePost.Content.ReadAsStringAsync();
        var details = JsonSerializer.Deserialize<Microsoft.AspNetCore.Mvc.ProblemDetails>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        // Assert
        Assert.That(responsePost.StatusCode, Is.EqualTo(HttpStatusCode.UnprocessableEntity));
        Assert.That(details, Is.Not.Null);
        Assert.That(details.Title, Is.EqualTo("Invalid request body"));
        Assert.That(details.Status, Is.EqualTo(422));
        Assert.That(details.Detail, Is.EqualTo("The request body is invalid or missing."));
        var errorCode = details.Extensions["errorCode"];
        Assert.That(errorCode, Is.Not.Null);
        var errorCodeString = ((JsonElement)errorCode).GetString();
        Assert.That(errorCodeString, Is.EqualTo("NAME_REQUIRED"));
    }
}