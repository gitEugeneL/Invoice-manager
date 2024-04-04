namespace Contracts;

public class FileSaveEvent
{
    public required Guid OwnerId { get; init; }
    public required string Name { get; init; }
    public required byte[] File { get; init; }
}