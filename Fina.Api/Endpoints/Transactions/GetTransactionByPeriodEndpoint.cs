using Fina.Api.Common.Api;
using Fina.Core.Handlers;
using Fina.Core.Models;
using Fina.Core.Requests.Categories;
using Fina.Core.Responses;
using Fina.Core;
using Microsoft.AspNetCore.Mvc;
using Fina.Core.Requests.Transactions;

namespace Fina.Api.Endpoints.Transactions;

public class GetTransactionByPeriodEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
           => app.MapPost("/", HandleAsync)
            .WithName("Categories: Get All")
            .WithSummary("Recupera todas as categoria")
            .WithOrder(4)
            .Produces<PagedResponse<List<Category>?>>();

    private static async Task<IResult> HandleAsync(//ClaimsPrincipal user, 
                    ITransactionHandler handler,
                    [FromQuery] DateTime? startDate = null,
                    [FromQuery] DateTime? endDate = null,
                    [FromQuery] int pageNumber = Configuration.DefaultPageNumber,
                    [FromQuery] int pageSize = Configuration.DefaultPageSize
            )
    {
        var request = new GetTransactionByPeriodRequest
        {
            UserId = ApiConfiguration.UserId,
            PageSize = pageSize,
            PageNumber = pageNumber,
            StartDate = startDate,
            EndDate = endDate
        };

        var result = await handler.GetByPeriodAsync(request);

        return result.IsSuccess
            ? TypedResults.Ok(result)
            : TypedResults.BadRequest(result);

    }
