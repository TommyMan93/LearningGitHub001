using CACIB.CREW.Api.Core.Route;
using CACIB.CREW.Api.Core.Service;
using CACIB.CREW.Api.Features.Referential.Model.Response;
using CACIB.CREW.Api.Infrastructure;
using CREW.Data.Enums.Referential;
using MediatR;
using Microsoft.AspNetCore.OutputCaching;

namespace CACIB.CREW.Api.Features.Referential.Handlers;

public class GetReferentialByReferentialType
{
    public class Endpoint : IEndpoint
    {
        // We expose the endpoints for Referential data
        public void ConfigureRoute(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet(ApiRouteConstants.ReferentialRoutes.GetByType, [OutputCache(PolicyName = "ExpireDay")] async (string[] types, bool isUkLabel, IMediator sender) =>
            {
                GetReferentialByReferentialTypeResponse response = await sender.Send(new Query(types, isUkLabel));
                return Results.Ok(response);
            })
            .Produces<GetReferentialByReferentialTypeResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .WithOpenApi(operation => new(operation)
            {
                OperationId = "GetReferentialDataForTypes",
                Summary = "Get Referential Data For Types Endpoint",
                Description = "Referential Endpoint to get referential data by passing in a list of referential entities " +
                            "as query params and a boolean to indicate if English is required",
            })
            .WithName("GetReferentialDataForTypes")
            .WithTags("Referential");
        }
    }

    public record Query : IRequest<GetReferentialByReferentialTypeResponse>
    {
        public string[] Types { get; init; }
        public bool IsUkLabel { get; init; }

        public Query(string[] types, bool isUkLabel)
        {
            Types = types;
            IsUkLabel = isUkLabel;
        }
    }

    public class Handler(IReferentialService srv) : IRequestHandler<Query, GetReferentialByReferentialTypeResponse>
    {
        private readonly IReferentialService _srv = srv;

        public async Task<GetReferentialByReferentialTypeResponse> Handle(Query request, CancellationToken cancellationToken)
        {
            // We convert the String Array to an Enum ReferentialType Array
            IEnumerable<ReferentialType> referentialTypesEnumArray = request.Types
                                                            .Select(x => Enum.Parse<ReferentialType>(x));

            GetReferentialByReferentialTypeResponse response = new()
            {
                Data = await _srv.GetReferentialByTypes(referentialTypesEnumArray.ToArray(), request.IsUkLabel)
            };

            return response;
        }
    }
}