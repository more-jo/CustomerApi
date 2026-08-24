namespace CustomerApi;

public record CreateCustomerRequest(string Name) : ICustomer;