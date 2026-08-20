namespace CustomerApi;

using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

public class ExceptionHandlingMiddleware
{
  private readonly RequestDelegate _next;
  private readonly ILogger<ExceptionHandlingMiddleware> _logger;

  // Loaded in the program in a dedicated middleware assignment call.
  public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
  {
    _next = next;
    _logger = logger;
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
      string details = string.Empty;

      if (ex is BadHttpRequestException || ex is JsonException)
      {
        statusCode = StatusCodes.Status400BadRequest;
        title = "Bad request";
        type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
        errorCode = "BAD_REQUEST";

        details = "The request body is not valid JSON";
      }
      else
      {
        statusCode = StatusCodes.Status500InternalServerError;
        title = "An error occurred while processing your request.";
        type = "https://tools.ietf.org/html/rfc7231#section-6.6.1";
        errorCode = "INTERNAL_ERROR";

        details = "An unexpected error occurred.";
      }

      ProblemDetails problemDetails = new ProblemDetails
      {
        Title = title,
        // Lesson learned: Never expose with Detail = ex.Message, — it can contain connection strings, paths or table names.
        Detail = details,
        Status = statusCode,
        Type = type,
        Instance = httpContext.Request.Path,
        Extensions = {
            ["errorCode"] = errorCode,
            ["traceId"] = httpContext.TraceIdentifier
          }
      };

      httpContext.Response.StatusCode = statusCode;
      httpContext.Response.ContentType = "application/problem+json";

      if (statusCode >= StatusCodes.Status500InternalServerError)
      {
        _logger.LogError(ex, "Unhandled exception on {Path}, traceId {TraceId}", httpContext.Request.Path, httpContext.TraceIdentifier);
      }
      else
      {
        _logger.LogWarning("Invalid request on {Path}, traceId {TraceId}", httpContext.Request.Path, httpContext.TraceIdentifier);
      }

      await httpContext.Response.WriteAsJsonAsync(problemDetails);
    }
  }
}
