using MediatR;
using TodoCharacters.Application.Abstractions;
using TodoCharacters.Application.Common;
using TodoCharacters.Application.Tasks.Dtos;

namespace TodoCharacters.Application.Tasks.Queries;

public sealed class GetTasksQuery : IRequest<Result<TasksGroupedDto>>
{
}

public sealed class GetTasksQueryHandler : IRequestHandler<GetTasksQuery, Result<TasksGroupedDto>>
{
    private readonly ITaskRepository _repository;

    public GetTasksQueryHandler(ITaskRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<TasksGroupedDto>> Handle(GetTasksQuery request, CancellationToken cancellationToken)
    {
        var tasks = await _repository.GetAllWithSubtasksAsync(cancellationToken);
        var roots = tasks.Where(t => t.ParentTaskId is null).ToList();
        return Result<TasksGroupedDto>.Success(TaskMappings.ToGroupedDto(roots));
    }
}
