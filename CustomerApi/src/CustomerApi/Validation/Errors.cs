namespace CustomerApi;

public static class Errors
{
  public static Error AccountNotFound { get; } = new("AccountNotFound", ErrorType.NotFound, "Account not found.");
  public static Error Invalid { get; } = new("InputInvalid", ErrorType.Validation, "Input is not valid.");
}

public enum ErrorType { NotFound, Validation }

public record Error(string Id, ErrorType Type, string Description);
