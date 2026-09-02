using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json;
using System.Net.Http.Json;

namespace CustomerApi.Tests;

public class PutCustomerTests
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
    public async Task PutCustomer_UpdateUserName_Returns204()
    {
        // Arrange
        var customer1 = await _testContextManager.CreateCustomerAsync(_client, "Alice");

        var expectation = "Alice updated";

        var updatedValue = new { Name = expectation };
        var jsonValue = JsonSerializer.Serialize(updatedValue);
        var httpContent = new StringContent(jsonValue, System.Text.Encoding.UTF8, "application/json");

        // Act
        var responsePut = await _client.PutAsync($"/customers/{customer1.Id}", httpContent);
        var responseGet = await _client.GetAsync($"/customers/{customer1.Id}");

        // Assert
        Assert.That(responsePut.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        Assert.That(responseGet.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var responseString = await responseGet.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var customer = JsonSerializer.Deserialize<CustomerResponse>(responseString, options);
        Assert.That(customer, Is.Not.Null);
        Assert.That(customer.Id, Is.EqualTo(customer1.Id));
        Assert.That(customer.Name, Is.EqualTo(expectation));
    }

    [Test]
    public async Task PutCustomer_UpdateUserNameUnnecessaryId_Returns204()
    {
        // Arrange
        var customer1 = await _testContextManager.CreateCustomerAsync(_client, "Alice");

        var updatedValue = new { Id = 999, Name = "Alice updated" };
        var jsonValue = JsonSerializer.Serialize(updatedValue);
        var httpContent = new StringContent(jsonValue, System.Text.Encoding.UTF8, "application/json");

        // Act 
        var response = await _client.PutAsync($"/customers/{customer1.Id}", httpContent);
        var responseGet = await _client.GetAsync($"/customers/{customer1.Id}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        Assert.That(responseGet.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var responseString = await responseGet.Content.ReadAsStringAsync();
        var option = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var customer = JsonSerializer.Deserialize<CustomerResponse>(responseString, option);
        Assert.That(customer, Is.Not.Null);
        Assert.That(customer.Id, Is.EqualTo(customer1.Id));
        Assert.That(customer.Name, Is.EqualTo(updatedValue.Name));
    }

    [Test]
    public async Task PutCustomer_WhenAbsentId_Returns404()
    {
        // Arrange
        var updatedValue = new { Name = "Alice updated" };
        var jsonValue = JsonSerializer.Serialize(updatedValue);
        var httpContent = new StringContent(jsonValue, System.Text.Encoding.UTF8, "application/json");

        // Act 
        var response = await _client.PutAsync("/customers/999", httpContent);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task PutCustomer_WhenMalformedJson_ReturnsBadRequest()
    {
        // Arrange
        var customer1 = await _testContextManager.CreateCustomerAsync(_client, "Alice");
        var httpContent = new StringContent("{ Name: }", System.Text.Encoding.UTF8, "application/json");

        // Act 
        var response = await _client.PutAsync($"/customers/{customer1.Id}", httpContent);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task PutCustomer_WhenNameIsMissing_ReturnsUnprocessableEntity()
    {
        // Arrange
        var customer1 = await _testContextManager.CreateCustomerAsync(_client, "Alice");
        var httpContent = new StringContent("{ }", System.Text.Encoding.UTF8, "application/json");

        // Act 
        var response = await _client.PutAsync($"/customers/{customer1.Id}", httpContent);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UnprocessableEntity));
    }

    [Test]
    public async Task PutCustomer_WhenNameIsMissing_Returns422WithProblemDetails()
    {
        // Arrange
        var customer1 = await _testContextManager.CreateCustomerAsync(_client, "Alice");
        var httpContent = new StringContent("{ }", System.Text.Encoding.UTF8, "application/json");

        // Act 
        var response = await _client.PutAsync($"/customers/{customer1.Id}", httpContent);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UnprocessableEntity));

        var content = await response.Content.ReadAsStringAsync();
        var details = JsonSerializer.Deserialize<Microsoft.AspNetCore.Mvc.ProblemDetails>(
            content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );
        Assert.That(details, Is.Not.Null);
        Assert.That(details.Title, Is.EqualTo("Invalid request body"));
        Assert.That(details.Status, Is.EqualTo(422));
        Assert.That(details.Detail, Is.EqualTo("The request body is invalid or missing."));
        var errorCode = details.Extensions["errorCode"]?.ToString();
        Assert.That(errorCode, Is.EqualTo("NAME_REQUIRED"));
    }

    [Test]
    public async Task PutCustomer_WhenNameIsEmpty_ReturnsUnprocessableEntity()
    {
        // Arrange
        var customer1 = await _testContextManager.CreateCustomerAsync(_client, "Alice");

        var updatedValue = new { Name = string.Empty };
        var jsonValue = JsonSerializer.Serialize(updatedValue);
        var httpContent = new StringContent(jsonValue, System.Text.Encoding.UTF8, "application/json");

        // Act 
        var response = await _client.PutAsync($"/customers/{customer1.Id}", httpContent);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UnprocessableEntity));
    }
}