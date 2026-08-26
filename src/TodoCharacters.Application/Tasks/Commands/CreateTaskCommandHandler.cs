using TodoCharacters.Application.Abstractions;
using TodoCharacters.Application.Common;
using TodoCharacters.Application.Tasks.Dtos;
using TodoCharacters.Domain.Entities;
using MediatR;

namespace TodoCharacters.Application.Tasks.Commands;

public sealed class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, Result<TaskDto>>
{
    private readonly ITaskRepository _repository;

    public CreateTaskCommandHandler(ITaskRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<TaskDto>> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.TarefaPaiId is null)
            {
                var root = TaskItem.CreateRoot(request.Titulo, request.Descricao, DateTimeOffset.UtcNow);
                await _repository.AddAsync(root, cancellationToken);
                await _repository.SaveChangesAsync(cancellationToken);
                return Result<TaskDto>.Success(TaskMappings.ToDto(root));
            }

            var parent = await _repository.GetByIdAsync(request.TarefaPaiId.Value, cancellationToken);
            if (parent is null)
            {
                return Result<TaskDto>.Failure(new Error(
                    "TAREFA_PAI_NAO_ENCONTRADA",
                    "Tarefa pai não encontrada.",
                    ErrorType.NotFound));
            }

            var createResult = TaskItem.CreateSubtask(
                parent,
                request.Titulo,
                request.Descricao,
                DateTimeOffset.UtcNow,
                out var subtask);

            if (!createResult.IsSuccess)
            {
                return Result<TaskDto>.Failure(new Error(
                    createResult.ErrorCode!,
                    createResult.Message!,
                    ErrorType.Conflict));
            }

            await _repository.AddAsync(subtask!, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            return Result<TaskDto>.Success(TaskMappings.ToDto(subtask!));
        }
        catch (ArgumentException ex)
        {
            return Result<TaskDto>.Failure(new Error("VALIDATION_ERROR", ex.Message, ErrorType.Validation));
        }
    }
}
