using Carter;
using MediatR;
using StorageApi.Helpers;
using StorageApi.Services;

namespace StorageApi.Features;

public class DownloadFile : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/v1/storage/{fileName}", async (string fileName, HttpContext context, ISender sender) =>
            {
                var query = new Query(TokenService.ReadUserIdFromToken(context).ToString(), fileName);
                return await sender.Send(query);
            })
            .RequireAuthorization(AppConstants.BaseAuthPolicy);
    }

    public sealed record Query(
        string CurrentUserId,
        string FileName
    ) : IRequest<IResult>;
    
    internal sealed class Handler(IStorageService storageService) : IRequestHandler<Query, IResult>
    {
        public async Task<IResult> Handle(Query query, CancellationToken ct)
        {
            if (!await storageService.BucketExists(query.CurrentUserId))
                return TypedResults.NotFound();
            
            var objectsList = await storageService.GetObjectsList(query.CurrentUserId);
            if (!objectsList.Any(item => item.Equals(query.FileName.ToLower())))
                return TypedResults.NotFound($"File: {query.FileName} not found");
            
            var stream = await storageService.DownloadFile(query.CurrentUserId, query.FileName);
            return TypedResults.File(stream, "application/pdf", query.FileName);
        }
    }
}