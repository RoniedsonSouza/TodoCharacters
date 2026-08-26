using TodoCharacters.Application.Abstractions;
using TodoCharacters.Application.Common;
using TodoCharacters.Application.Tasks.Dtos;
using MediatR;

namespace TodoCharacters.Application.Tasks.Commands;

public sealed class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, Result<TaskDto>>
{
    private readonly ITaskRepository _repository;

    public UpdateTaskCommandHandler(ITaskRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<TaskDto>> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _repository.GetByIdWithSubtasksAsync(request.Id, cancellationToken);
        if (task is null)
        {
            return Result<TaskDto>.Failure(new Error(
                "TAREFA_NAO_ENCONTRADA",
                "Tarefa não encontrada.",
                ErrorType.NotFound));
        }

        try
        {
            var updateResult = task.Update(request.Titulo, request.Descricao);
            if (!updateResult.IsSuccess)
            {
                return Result<TaskDto>.Failure(new Error(
                    updateResult.ErrorCode!,
                    updateResult.Message!,
                    ErrorType.Validation));
            }

            await _repository.UpdateAsync(task, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            return Result<TaskDto>.Success(TaskMappings.ToDto(task));
        }
        catch (ArgumentException ex)
        {
            return Result<TaskDto>.Failure(new Error("VALIDATION_ERROR", ex.Message, ErrorType.Validation));
        }
    }
}
