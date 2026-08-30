namespace CustomerApi;

public record PatchCustomerRequest(int Id, string? Name, bool? IsDeleted) : ICustomer;