using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json;
using System.Net.Http.Json;

namespace CustomerApi.Tests;

public class PostCustomerTests
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
    public async Task PostCustomer_Returns201WithLocationHeader()
    {
        // Arrange
        var customer1 = new { Name = "Alice" };
        var responsePostCustomer1 = await _client.PostAsJsonAsync("/customers", customer1);
        var responsePostCustomerObject = await GetCustomerFromResponse(responsePostCustomer1);
        Assume.That(responsePostCustomerObject, Is.Not.Null);

        var newCustomer = new { Name = "Charlie" };
        string newCustomerJson = JsonSerializer.Serialize(newCustomer);
        var httpContent = new StringContent(newCustomerJson, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/customers", httpContent);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        Assert.That(response.Headers.Location, Is.Not.Null);
        var expectedNewCustomerNumber = 2;
        Assert.That(response.Headers.Location, Is.EqualTo(new Uri($"/customers/{expectedNewCustomerNumber}", UriKind.Relative)));
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
        var customer1 = new { Name = "Alice" };
        var responsePostCustomer1 = await _client.PostAsJsonAsync("/customers", customer1);
        var responsePostCustomerObject = await GetCustomerFromResponse(responsePostCustomer1);
        Assume.That(responsePostCustomerObject, Is.Not.Null);

        var customer2 = new { Name = "Bob" };
        var responsePostCustomer2 = await _client.PostAsJsonAsync("/customers", customer2);
        var responsePostCustomerObject2 = await GetCustomerFromResponse(responsePostCustomer2);
        Assume.That(responsePostCustomerObject2, Is.Not.Null);

        string newCustomerJson = JsonSerializer.Serialize(new { Name = "Charlie" });
        var httpContent = new StringContent(newCustomerJson, System.Text.Encoding.UTF8, "application/json");

        // Act
        var responsePost = await _client.PostAsync("/customers", httpContent);

        var responseGet = await _client.GetAsync("/customers");

        // Assert
        Assert.That(responsePost.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        var content = await responseGet.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        Assert.That(responseGet.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var customers = JsonSerializer.Deserialize<List<Customer>>(content, options);
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

    private static async Task<Customer?> GetCustomerFromResponse(HttpResponseMessage response)
    {
        string responseJson = await response.Content.ReadAsStringAsync();
        JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };
        var customer = JsonSerializer.Deserialize<Customer>(responseJson, options);
        return customer;
    }
}