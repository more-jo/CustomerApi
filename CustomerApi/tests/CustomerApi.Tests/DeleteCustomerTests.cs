using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CustomerApi.Tests;

public class DeleteCustomerTests
{
    private TestContextManager _testContextManager;

    [SetUp]
    public async Task Setup()
    {
        _testContextManager = new TestContextManager();
    }

    [Test]
    public async Task DeleteAbsentCustomer_ReturnsNotFound()
    {
        // Arrange
        await using var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();

        // Act
        var deleteRepsonse = await client.DeleteAsync("/customers/999");

        // Assert
        Assert.That(deleteRepsonse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task DeleteCustomer_ExistingId_ReturnsDeletedCustomer()
    {
        // Arrange
        await using var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();

        var customerBeforeDeletion = await _testContextManager.CreateCustomerAsync(client, "Charlie");

        // Act
        var responseDelete = await client.DeleteAsync($"/customers/{customerBeforeDeletion.Id}");

        // assert
        var responseGetAfter = await client.GetAsync($"/customers/{customerBeforeDeletion.Id}");
        var customerAfterDeletion = await GetCustomerFromContent(responseGetAfter.Content);
        Assert.That(customerAfterDeletion, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(responseDelete.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(customerAfterDeletion.IsDeleted, Is.True);
        });
    }

    private async Task<Customer?> GetCustomerFromContent(HttpContent content)
    {
        var responseString = await content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return JsonSerializer.Deserialize<Customer>(responseString, options);
    }
}