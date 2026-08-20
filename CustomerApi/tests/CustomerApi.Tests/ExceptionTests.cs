using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Hosting;

namespace CustomerApi.Tests;

public class ExceptionTests
{
  [Test]
  public async Task UnhandledException_Returns500WithProblemDetails()
  {
    // Arrange 
    var factory = new WebApplicationFactory<Program>();
    var client = factory.CreateClient();

    // Act
    var response = await client.GetAsync("/throw");

    // Assert
    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
    var content = await response.Content.ReadAsStringAsync();
    var details = JsonSerializer.Deserialize<ProblemDetails>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    Assert.That(details, Is.Not.Null);
    Assert.That(details.Title, Is.EqualTo("An error occurred while processing your request."));
    Assert.That(details.Status, Is.EqualTo(500));
  }

  [Test]
  public async Task UnhandledException_ReturnsErrorWithoutRevealingDetails()
  {
    // Arrange 
    var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(b => b.UseEnvironment("Development"));
    var client = factory.CreateClient();

    // Act
    var response = await client.GetAsync("/throw");

    // Assert
    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
    var content = await response.Content.ReadAsStringAsync();
    var details = JsonSerializer.Deserialize<ProblemDetails>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    Assert.That(details, Is.Not.Null);
    Assert.That(details.Detail, Is.EqualTo("An unexpected error occurred."));
    Assert.That(details.Extensions.ContainsKey("traceId"), Is.True);
    Assert.That(details.Extensions["traceId"]?.ToString(), Is.Not.Empty);
  }

  [Test]
  public async Task ExceptionHandlingMiddleware_DoesNotAffectNormalRequests()
  {
    var factory = new WebApplicationFactory<Program>();
    var client = factory.CreateClient();

    // GET existierende Ressource
    var responseGet = await client.GetAsync("/customers/1");
    Assert.That(responseGet.StatusCode, Is.EqualTo(HttpStatusCode.OK));

    // POST neue Ressource
    var newCustomer = new { Name = "Test" };
    var json = JsonSerializer.Serialize(newCustomer);
    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
    var responsePost = await client.PostAsync("/customers", content);
    Assert.That(responsePost.StatusCode, Is.EqualTo(HttpStatusCode.Created));
  }

  [Test]
  public void ValidateCreateCustomerRequest_ReturnsNull()
  {
    // Arrange 
    var c = new CreateCustomerRequest("test");

    // Act
    var response = CustomerRequestValidator.ValidateCustomerName(c);

    // Assert
    Assert.That(response, Is.Null);
  }

  [Test]
  public void ValidateUpdateCustomerRequest_ReturnsNull()
  {
    // Arrange 
    var c = new UpdateCustomerRequest("test");

    // Act
    var response = CustomerRequestValidator.ValidateCustomerName(c);

    // Assert
    Assert.That(response, Is.Null);
  }

  [Test]
  public void ValidateCustomerName_Empty_ReturnsBadRequest()
  {
    // Arrange    
    UpdateCustomerRequest? update = null;
    CreateCustomerRequest? request = null;

    // Act
    var responseUpdate = CustomerRequestValidator.ValidateCustomerName(update);
    var responseRequest = CustomerRequestValidator.ValidateCustomerName(request);

    // Assert
    Assert.That(responseUpdate, Is.TypeOf(typeof(BadRequest)));
    Assert.That(responseRequest, Is.TypeOf(typeof(BadRequest)));
  }

  [Test]
  public void ValidateCustomerNameUpdate_EmptyName_ReturnsProblem()
  {
    // Arrange    
    UpdateCustomerRequest update = new UpdateCustomerRequest("");

    // Act
    var responseUpdate = CustomerRequestValidator.ValidateCustomerName(update);

    // Assert
    Assert.That(responseUpdate, Is.TypeOf(typeof(ProblemHttpResult)));
    var detailsUpdate = responseUpdate as ProblemHttpResult;
    Assert.That(detailsUpdate, Is.Not.Null);
    Assert.That(detailsUpdate.StatusCode, Is.EqualTo(422));
    Assert.That(detailsUpdate.ProblemDetails.Status, Is.EqualTo(422));
    Assert.That(detailsUpdate.ProblemDetails.Title, Is.EqualTo("Invalid request body"));
    Assert.That(detailsUpdate.ProblemDetails.Detail, Is.EqualTo("The request body is invalid or missing."));
    Assert.That(detailsUpdate.ProblemDetails.Extensions["errorCode"], Is.Not.Null);
    Assert.That(detailsUpdate.ProblemDetails.Extensions["errorCode"].ToString(), Is.EqualTo("NAME_REQUIRED"));
  }

  [Test]
  public void ValidateCustomerNameRequest_EmptyName_ReturnsProblem()
  {
    // Arrange
    CreateCustomerRequest request = new CreateCustomerRequest("");

    // Act
    var responseRequest = CustomerRequestValidator.ValidateCustomerName(request);

    // Assert
    Assert.That(responseRequest, Is.TypeOf(typeof(ProblemHttpResult)));
    var detailsUpdate = responseRequest as ProblemHttpResult;
    Assert.That(detailsUpdate, Is.Not.Null);
    Assert.That(detailsUpdate.StatusCode, Is.EqualTo(422));
    Assert.That(detailsUpdate.ProblemDetails.Status, Is.EqualTo(422));
    Assert.That(detailsUpdate.ProblemDetails.Title, Is.EqualTo("Invalid request body"));
    Assert.That(detailsUpdate.ProblemDetails.Detail, Is.EqualTo("The request body is invalid or missing."));
    Assert.That(detailsUpdate.ProblemDetails.Extensions["errorCode"], Is.Not.Null);
    Assert.That(detailsUpdate.ProblemDetails.Extensions["errorCode"].ToString(), Is.EqualTo("NAME_REQUIRED"));
  }
}