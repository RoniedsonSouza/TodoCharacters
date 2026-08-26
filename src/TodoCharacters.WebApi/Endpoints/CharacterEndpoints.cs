using MediatR;
using TodoCharacters.Application.Characters.Queries;
using TodoCharacters.WebApi.Mapping;

namespace TodoCharacters.WebApi.Endpoints;

public static class CharacterEndpoints
{
    public static RouteGroupBuilder MapCharacterEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/characters").WithTags("Characters");

        group.MapGet("/", async (int page, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetCharactersQuery { Page = page <= 0 ? 1 : page }, ct);
            return ResultToHttpMapper.ToHttpResult(result);
        });

        group.MapGet("/{id:int}", async (int id, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetCharacterByIdQuery { Id = id }, ct);
            return ResultToHttpMapper.ToHttpResult(result);
        });

        return group;
    }
}
