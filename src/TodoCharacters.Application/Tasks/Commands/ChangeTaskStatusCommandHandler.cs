using MediatR;
using TodoCharacters.Application.Abstractions;
using TodoCharacters.Application.Common;
using TodoCharacters.Application.Tasks.Dtos;
using TodoTaskStatus = TodoCharacters.Domain.Enums.TaskStatus;

namespace TodoCharacters.Application.Tasks.Commands;

public sealed class ChangeTaskStatusCommandHandler : IRequestHandler<ChangeTaskStatusCommand, Result<TaskDto>>
{
    private readonly ITaskRepository _repository;

    public ChangeTaskStatusCommandHandler(ITaskRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<TaskDto>> Handle(ChangeTaskStatusCommand request, CancellationToken cancellationToken)
    {
        if (!TaskMappings.TryParseStatus(request.Status, out var newStatus))
        {
            return Result<TaskDto>.Failure(new Error(
                "VALIDATION_ERROR",
                "Status inválido.",
                ErrorType.Validation));
        }

        var task = await _repository.GetByIdWithSubtasksAsync(request.Id, cancellationToken);
        if (task is null)
        {
            return Result<TaskDto>.Failure(new Error(
                "TAREFA_NAO_ENCONTRADA",
                "Tarefa não encontrada.",
                ErrorType.NotFound));
        }

        var previousStatus = task.Status;
        var changeResult = task.ChangeStatus(newStatus, task.Subtasks);
        if (!changeResult.IsSuccess)
        {
            return Result<TaskDto>.Failure(new Error(
                changeResult.ErrorCode!,
                changeResult.Message!,
                ErrorType.Conflict));
        }

        if (task.ParentTaskId.HasValue &&
            previousStatus == TodoTaskStatus.Completed &&
            newStatus != TodoTaskStatus.Completed)
        {
            var parent = await _repository.GetByIdWithSubtasksAsync(task.ParentTaskId.Value, cancellationToken);
            if (parent is not null)
            {
                parent.ReopenFromCompleted();
                await _repository.UpdateAsync(parent, cancellationToken);
            }
        }

        await _repository.UpdateAsync(task, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return Result<TaskDto>.Success(TaskMappings.ToDto(task));
    }
}
