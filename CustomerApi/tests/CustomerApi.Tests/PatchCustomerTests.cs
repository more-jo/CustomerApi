using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;

namespace CustomerApi.Tests;

public class PatchCustomerTests
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
    public async Task PatchCustomer_RestoreCustomer_Returns204()
    {
        // Arrange
        var newCustomer = await _testContextManager.CreateCustomerAsync(_client, "Alice");
        Assume.That(newCustomer, Is.Not.Null);

        var newCustomerGetResponse = await _client.GetAsync($"/customers/{newCustomer.Id}");
        var customerGet = await _testContextManager.GetCustomerFromResponse(newCustomerGetResponse);
        Assume.That(customerGet, Is.Not.Null);
        Assume.That(customerGet.IsDeleted, Is.False);

        var responseDelete = await _client.DeleteAsync($"/customers/{newCustomer.Id}");
        Assume.That(responseDelete.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var deletedCustomerGetResponse = await _client.GetAsync($"/customers/{newCustomer.Id}");
        var deletedCustomer = await _testContextManager.GetCustomerFromResponse(deletedCustomerGetResponse);
        Assume.That(deletedCustomer, Is.Not.Null);
        Assume.That(deletedCustomer.IsDeleted, Is.True);

        // Act
        var httpContent = new PatchCustomerRequest(deletedCustomer.Name, false);
        var response = await _client.PatchAsJsonAsync($"/customers/{newCustomer.Id}", httpContent);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var customerAfterPatchResponse = await _client.GetAsync($"/customers/{newCustomer.Id}");
        var customerAfterPatch = await _testContextManager.GetCustomerFromResponse(customerAfterPatchResponse);
        Assert.That(customerAfterPatch, Is.Not.Null);
        Assert.That(customerAfterPatch.IsDeleted, Is.False);
    }

    [Test]
    public async Task PatchCustomer_ChangeCustomerName_Returns204()
    {
        // Arrange
        var newCustomer = await _testContextManager.CreateCustomerAsync(_client, "Alice");
        Assume.That(newCustomer, Is.Not.Null);

        var newCustomerGetResponse = await _client.GetAsync($"/customers/{newCustomer.Id}");
        var customerGet = await _testContextManager.GetCustomerFromResponse(newCustomerGetResponse);
        Assume.That(customerGet, Is.Not.Null);
        Assume.That(customerGet.Name, Is.EqualTo(newCustomer.Name));
        const bool EXPECTED_DELETED_STATE = false;
        Assume.That(customerGet.IsDeleted, Is.EqualTo(EXPECTED_DELETED_STATE));

        // Act
        const string EXPECTED_NAME = "newName";
        var httpContent = new PatchCustomerRequest(EXPECTED_NAME, false);
        var response = await _client.PatchAsJsonAsync($"/customers/{newCustomer.Id}", httpContent);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var customerAfterPatchResponse = await _client.GetAsync($"/customers/{newCustomer.Id}");
        var customerAfterPatch = await _testContextManager.GetCustomerFromResponse(customerAfterPatchResponse);
        Assert.That(customerAfterPatch, Is.Not.Null);
        Assert.That(customerAfterPatch.IsDeleted, Is.EqualTo(EXPECTED_DELETED_STATE));
        Assert.That(customerAfterPatch.Name, Is.EqualTo(EXPECTED_NAME));
    }

    [Test]
    public async Task PatchCustomer_AbsentCustomer_Returns404()
    {
        // Arrange
        const int NON_EXISTING_CUSTOMER_ID = 999;

        // Act
        var httpContent = new PatchCustomerRequest("absent customer", false);
        var response = await _client.PatchAsJsonAsync($"/customers/{NON_EXISTING_CUSTOMER_ID}", httpContent);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task PatchCustomer_NameOnly_LeavesIsDeletedUnchanged()
    {
        // Arrange
        var newCustomer = await _testContextManager.CreateCustomerAsync(_client, "Alice");

        var newCustomerGetResponse = await _client.GetAsync($"/customers/{newCustomer.Id}");
        var customerGet = await _testContextManager.GetCustomerFromResponse(newCustomerGetResponse);
        Assume.That(customerGet, Is.Not.Null);
        Assume.That(customerGet.Name, Is.EqualTo(newCustomer.Name));
        const bool EXPECTED_DELETED_STATE_AFTER_CREATION = false;
        Assume.That(customerGet.IsDeleted, Is.EqualTo(EXPECTED_DELETED_STATE_AFTER_CREATION));

        var deleteResponse = await _client.DeleteAsync($"/customers/{newCustomer.Id}");
        Assume.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        // Act
        var body = new PatchCustomerRequest("Renamed", null); // only name; IsDeleted = null
        var response = await _client.PatchAsJsonAsync($"/customers/{newCustomer.Id}", body);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var getResponse = await _client.GetAsync($"/customers/{newCustomer.Id}");
        var after = await _testContextManager.GetCustomerFromResponse(getResponse);
        Assert.That(after, Is.Not.Null);
        Assert.That(after.Name, Is.EqualTo("Renamed"));
        Assert.That(after.IsDeleted, Is.True);
    }
}