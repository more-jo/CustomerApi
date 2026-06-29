
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

public class ExceptionHandlingMiddleware
{
  private readonly RequestDelegate _next;

  public ExceptionHandlingMiddleware(RequestDelegate next)
  {
    _next = next;
  }

  public async Task InvokeAsync(HttpContext httpContext)
  {
    try
    {
      await _next(httpContext);
    }
    catch (Exception ex)
    {
      int statusCode;
      string title;
      string type;
      object? errorCode;

      if (ex is BadHttpRequestException || ex is JsonException)
      {
        statusCode = StatusCodes.Status400BadRequest;
        title = "Bad request";
        type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
        errorCode = "BAD_REQUEST";
      }
      else
      {
        statusCode = StatusCodes.Status500InternalServerError;
        title = "An error occurred while processing your request.";
        type = "https://tools.ietf.org/html/rfc7231#section-6.6.1";
        errorCode = "INTERNAL_ERROR";
      }

      httpContext.Response.StatusCode = statusCode;
      httpContext.Response.ContentType = "application/problem+json";

      var problemDetails = new ProblemDetails
      {
        Title = title,
        Detail = ex.Message,
        Status = statusCode,
        Type = type,
        Instance = httpContext.Request.Path,
        Extensions = { ["errorCode"] = errorCode }
      };

      await httpContext.Response.WriteAsJsonAsync(problemDetails);
    }
  }
}
