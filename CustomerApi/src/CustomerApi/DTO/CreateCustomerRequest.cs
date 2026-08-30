namespace CustomerApi;

public record CreateCustomerRequest(int Id, string Name) : ICustomer;