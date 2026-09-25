using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace CustomerApi.Tests;

public class DeleteCustomerTests
{
    private WebApplicationFactory<Program> _factory = null!;
    private System.Net.Http.HttpClient _client = null!;
    private TestContextManager _testContextManager = null!;

    [SetUp]
    public void Setup()
    {
        _factory = new WebApplicationFactory<Program>();
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CustomerDbContext>();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        }
        _client = _factory.CreateClient();

        _testContextManager = new TestContextManager();
    }

    [TearDown]
    public async Task TearDown()
    {
        _client?.Dispose();
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
        var customerAfterDeletion = await _testContextManager.GetCustomerFromResponse(responseGetAfter);
        Assert.That(customerAfterDeletion, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(responseDelete.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(customerAfterDeletion.IsDeleted, Is.True);
        });
    }
}