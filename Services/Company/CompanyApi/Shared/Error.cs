namespace CompanyApi.Shared;

public record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);

    public static readonly Error NullValue = new("NullValue", "The specified result value is null");

    public static readonly Error ConditionNotMet = new("ConditionNotMet", "The specified condition was not metF");
}
