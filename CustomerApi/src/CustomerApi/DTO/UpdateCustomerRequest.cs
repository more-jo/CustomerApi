namespace CustomerApi;

public record UpdateCustomerRequest(string Name) : IHasCustomerName;