namespace CustomerApi;

public record UpdateCustomerRequest(string Name) : CustomerName(Name);