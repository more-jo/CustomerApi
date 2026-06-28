using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json;

namespace CustomerApi.Tests;

public class PutCustomerTests
{
    private WebApplicationFactory<Program>? _factory;

    /// <summary>
    /// This cannot be used for database approach : 
    /// factory gets discarded. 
    /// Database is created anew (empty/seedless) when called.
    /// </summary>
    private async Task<HttpClient> CreateClient()
    {
        _factory = new WebApplicationFactory<Program>();
        return _factory.CreateClient();
    }

    [Test]
    public async Task PutCustomer_UpdateUserName_Returns204()
    {
        // Arrange
        var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();
        var expectation = "Alice updated";

        var updatedValue = new { Name = expectation };
        var jsonValue = JsonSerializer.Serialize(updatedValue);
        var httpContent = new StringContent(jsonValue, System.Text.Encoding.UTF8, "application/json");

        // Act 
        var responsePut = await client.PutAsync("/customers/1", httpContent);
        var responseGet = await client.GetAsync("/customers/1");

        // Assert
        Assert.That(responsePut.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        Assert.That(responseGet.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var responseString = await responseGet.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var customer = JsonSerializer.Deserialize<Customer>(responseString, options);
        Assert.That(customer, Is.Not.Null);
        Assert.That(customer.Id, Is.EqualTo(1));
        Assert.That(customer.Name, Is.EqualTo(expectation));
    }

    [Test]
    public async Task PutCustomer_UpdateUserNameUnnecessaryId_Returns204()
    {
        // Arrange
        var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();

        var updatedValue = new { Id = 999, Name = "Alice updated" };
        var jsonValue = JsonSerializer.Serialize(updatedValue);
        var httpContent = new StringContent(jsonValue, System.Text.Encoding.UTF8, "application/json");

        // Act 
        var response = await client.PutAsync("/customers/1", httpContent);
        var responseGet = await client.GetAsync("/customers/1");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        Assert.That(responseGet.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var responseString = await responseGet.Content.ReadAsStringAsync();
        var option = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var customer = JsonSerializer.Deserialize<Customer>(responseString, option);
        Assert.That(customer, Is.Not.Null);
        Assert.That(customer.Id, Is.EqualTo(1));
        Assert.That(customer.Name, Is.EqualTo(updatedValue.Name));
    }

    [Test]
    public async Task PutCustomer_WhenAbsentId_Returns404()
    {
        // Arrange
        var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();

        var updatedValue = new { Name = "Alice updated" };
        var jsonValue = JsonSerializer.Serialize(updatedValue);
        var httpContent = new StringContent(jsonValue, System.Text.Encoding.UTF8, "application/json");

        // Act 
        var response = await client.PutAsync("/customers/999", httpContent);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task PutCustomer_WhenMalformedJson_ReturnsBadRequest()
    {
        // Arrange
        var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();

        var httpContent = new StringContent("{ Name: }", System.Text.Encoding.UTF8, "application/json");

        // Act 
        var response = await client.PutAsync("/customers/1", httpContent);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task PutCustomer_WhenNameIsMissing_ReturnsUnprocessableEntity()
    {
        // Arrange
        var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();

        var httpContent = new StringContent("{ }", System.Text.Encoding.UTF8, "application/json");

        // Act 
        var response = await client.PutAsync("/customers/1", httpContent);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UnprocessableEntity));
    }

    [Test]
    public async Task PutCustomer_WhenNameIsMissing_Returns422WithProblemDetails()
    {
        // Arrange
        var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();

        var httpContent = new StringContent("{ }", System.Text.Encoding.UTF8, "application/json");

        // Act 
        var response = await client.PutAsync("/customers/1", httpContent);

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
        var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();

        var updatedValue = new { Name = string.Empty };
        var jsonValue = JsonSerializer.Serialize(updatedValue);
        var httpContent = new StringContent(jsonValue, System.Text.Encoding.UTF8, "application/json");

        // Act 
        var response = await client.PutAsync("/customers/1", httpContent);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UnprocessableEntity));
    }

    [TearDown]
    public void TearDown()
    {
        _factory?.Dispose();
    }
}