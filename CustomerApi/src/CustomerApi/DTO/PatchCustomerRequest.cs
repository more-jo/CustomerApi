namespace CustomerApi;

public record PatchCustomerRequest(string? Name, bool? IsDeleted);