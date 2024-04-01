using Contracts;
using FileGeneratorApi.Services;
using FileGeneratorApi.Utils;
using MassTransit;
using QuestPDF.Fluent;

namespace FileGeneratorApi.Consumers;

public sealed class CreateFileConsumer(ICompanyService companyService) : IConsumer<FileCreateEvent>
{
    public async Task Consume(ConsumeContext<FileCreateEvent> context)
    {
        var invoice = context.Message;
        var sellerCompany = await companyService.GetCompanyAsync(context.Message.SellerCompanyId);
        var bayerCompany = await companyService.GetCompanyAsync(context.Message.BuyerCompanyId);
        
        new DocumentGenerator(invoice, sellerCompany, bayerCompany)
            .GeneratePdf("test.pdf");
    }
}