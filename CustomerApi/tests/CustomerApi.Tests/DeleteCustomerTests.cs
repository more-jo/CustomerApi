using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CustomerApi.Tests;

public class DeleteCustomerTests
{
    private WebApplicationFactory<Program> _factory = null!;
    private System.Net.Http.HttpClient _client;
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
    public async Task DeleteAbsentCustomer_ReturnsNotFound()
    {
        // Act
        var deleteRepsonse = await _client.DeleteAsync("/customers/999");

        // Assert
        Assert.That(deleteRepsonse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task DeleteCustomer_ExistingId_ReturnsDeletedCustomer()
    {
        // Arrange
        var customerBeforeDeletion = await _testContextManager.CreateCustomerAsync(_client, "Charlie");

        // Act
        var responseDelete = await _client.DeleteAsync($"/customers/{customerBeforeDeletion.Id}");

        // assert
        var responseGetAfter = await _client.GetAsync($"/customers/{customerBeforeDeletion.Id}");
        var customerAfterDeletion = await GetCustomerFromContent(responseGetAfter.Content);
        Assert.That(customerAfterDeletion, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(responseDelete.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(customerAfterDeletion.IsDeleted, Is.True);
        });
    }

    private async Task<CustomerResponse?> GetCustomerFromContent(HttpContent content)
    {
        var responseString = await content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return JsonSerializer.Deserialize<CustomerResponse>(responseString, options);
    }
}