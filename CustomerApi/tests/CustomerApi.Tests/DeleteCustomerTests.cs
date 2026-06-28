using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CustomerApi.Tests;

public class DeleteCustomerTests
{
    [Test]
    public async Task DeleteCustomer_ExistingId_Returns204()
    {
        // Arrange
        await using var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();

        // act
        var responseGetBefore = await client.GetAsync("/customers/1");
        var responseDelete = await client.DeleteAsync("/customers/1");
        var responseGetAfter = await client.GetAsync("/customers/1");

        // assert
        Assert.That(responseGetBefore.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(responseDelete.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        Assert.That(responseGetAfter.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
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
}