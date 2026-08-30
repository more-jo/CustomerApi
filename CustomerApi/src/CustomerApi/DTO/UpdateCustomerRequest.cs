namespace CustomerApi;

public record UpdateCustomerRequest(int Id, string Name) : ICustomer;
public record CustomerPatchRequest(int Id, string? Name, bool? IsDeleted) : ICustomer;