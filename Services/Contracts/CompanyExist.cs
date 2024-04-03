namespace Contracts;

public sealed record CompanyExistRequest
{
    public required Guid CompanyId { get; init; }
}

public sealed record CompanyExistResponse
{
    public required bool CompanyExists { get; init; }
}