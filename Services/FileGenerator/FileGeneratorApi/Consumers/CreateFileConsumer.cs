using Contracts;
using FileGeneratorApi.Services;
using FileGeneratorApi.Utils;
using MassTransit;
using QuestPDF.Fluent;

namespace FileGeneratorApi.Consumers;

public sealed class CreateFileConsumer(
    ICompanyService companyService,
    IPublishEndpoint publishEndpoint
) : IConsumer<FileCreateEvent>
{
    public async Task Consume(ConsumeContext<FileCreateEvent> context)
    {
        var createEvent = context.Message;
        var sellerCompany = await companyService.GetCompanyAsync(context.Message.SellerCompanyId);
        var bayerCompany = await companyService.GetCompanyAsync(context.Message.BuyerCompanyId);
        
        var file = new DocumentGenerator(createEvent, sellerCompany, bayerCompany)
            .GeneratePdf();

        await publishEndpoint.Publish(
            new FileSaveEvent
            {
                OwnerId = context.Message.OwnerId,
                Name = createEvent.Number,
                File = file
            },
            context.CancellationToken);
    }
}