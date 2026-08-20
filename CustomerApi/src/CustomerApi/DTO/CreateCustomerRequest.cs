namespace CustomerApi;

public record CreateCustomerRequest(string Name) : CustomerName(Name);