namespace CompanyApi.Shared;

public abstract class Errors
{
    public sealed record NotFound(string Name, object Key)
        : Error(Code: $"{Name}.Null", Message: $"Entity: ({Key}) not found");

    public sealed record Validation(string Name, string ValidationResult)
        : Error(Code: $"{Name}.Validation", Message: ValidationResult);
}