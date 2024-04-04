using Contracts;
using MassTransit;
using StorageApi.Services;

namespace StorageApi.Consumers;

public class SaveFileConsumer(
    IStorageService storageService
) : IConsumer<FileSaveEvent>
{
    public async Task Consume(ConsumeContext<FileSaveEvent> context)
    {
        var bucketName = context.Message.OwnerId.ToString();
        var fileName = $"{context.Message.Name}.pdf";
        
        if (!await storageService.BucketExists(bucketName))
            await storageService.CreateBucket(bucketName);
        
        await storageService
            .UploadFile(bucketName, fileName, context.Message.File);
    }
}