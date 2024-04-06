using Carter;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using StorageApi.Helpers;
using StorageApi.Services;

namespace StorageApi.Features;

public class GetFileList : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/v1/storage", async (HttpContext context, ISender sender) =>
            {
                var query = new Query(TokenService.ReadUserIdFromToken(context).ToString());
                return await sender.Send(query);
            })
            .RequireAuthorization(AppConstants.BaseAuthPolicy);
    }

    public sealed record Query(
        string CurrentUserId
    ) : IRequest<Results<Ok<List<string>>, NotFound>>;
    
    internal sealed class Handler(IStorageService storageService) : IRequestHandler<Query,Results<Ok<List<string>>, NotFound>>
    {
        public async Task<Results<Ok<List<string>>, NotFound>> Handle(Query query, CancellationToken ct)
        {
            if (!await storageService.BucketExists(query.CurrentUserId))
                return TypedResults.NotFound();
         
            var result = await storageService.GetObjectsList(query.CurrentUserId);
            return TypedResults.Ok(result);
        }
    }
}