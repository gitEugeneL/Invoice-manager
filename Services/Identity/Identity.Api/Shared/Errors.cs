namespace Identity.Api.Shared;

public abstract class Errors
{
    public sealed record Validation(string Code, string Message) 
        : Error($"{Code}.Validation", Message);

    public sealed record NotFound(string Code, string Message)
        : Error($"{Code}.Null", $"Entity: ({Message}) not found");

    public sealed record Conflict(string Code, string Message)
        : Error($"{Code}.Conflict", $"Entity: ({Message}) already exists");

    public sealed record Credentials(string Code)
        : Error($"{Code}.InvalidCredentials", "Invalid Credentials");
}