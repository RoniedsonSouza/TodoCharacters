using MediatR;
using TodoCharacters.Application.Tasks.Commands;
using TodoCharacters.Application.Tasks.Dtos;
using TodoCharacters.Application.Tasks.Queries;
using TodoCharacters.WebApi.Mapping;

namespace TodoCharacters.WebApi.Endpoints;

public static class TaskEndpoints
{
    public static RouteGroupBuilder MapTaskEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tasks").WithTags("Tasks");

        group.MapGet("/", async (IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetTasksQuery(), ct);
            return ResultToHttpMapper.ToHttpResult(result);
        });

        group.MapPost("/", async (CreateTaskRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new CreateTaskCommand
            {
                Titulo = request.Titulo,
                Descricao = request.Descricao,
                TarefaPaiId = request.TarefaPaiId
            }, ct);

            return result.IsSuccess
                ? Results.Created($"/api/tasks/{result.Value!.Id}", result.Value)
                : ResultToHttpMapper.ToHttpResult(result);
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateTaskRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new UpdateTaskCommand
            {
                Id = id,
                Titulo = request.Titulo,
                Descricao = request.Descricao
            }, ct);

            return ResultToHttpMapper.ToHttpResult(result);
        });

        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new DeleteTaskCommand { Id = id }, ct);
            return result.IsSuccess ? Results.NoContent() : ResultToHttpMapper.ToHttpResult(result);
        });

        group.MapPatch("/{id:guid}/status", async (Guid id, ChangeTaskStatusRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new ChangeTaskStatusCommand
            {
                Id = id,
                Status = request.Status
            }, ct);

            return ResultToHttpMapper.ToHttpResult(result);
        });

        return group;
    }
}
