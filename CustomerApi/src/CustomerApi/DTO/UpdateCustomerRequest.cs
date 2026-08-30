namespace CustomerApi;

public record UpdateCustomerRequest(int Id, string Name) : ICustomer;