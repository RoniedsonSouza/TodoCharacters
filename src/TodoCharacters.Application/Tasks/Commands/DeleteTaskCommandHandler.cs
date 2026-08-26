using MediatR;
using TodoCharacters.Application.Abstractions;
using TodoCharacters.Application.Common;

namespace TodoCharacters.Application.Tasks.Commands;

public sealed class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand, Result>
{
    private readonly ITaskRepository _repository;

    public DeleteTaskCommandHandler(ITaskRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _repository.GetByIdWithSubtasksAsync(request.Id, cancellationToken);
        if (task is null)
        {
            return Result.Failure(new Error(
                "TAREFA_NAO_ENCONTRADA",
                "Tarefa não encontrada.",
                ErrorType.NotFound));
        }

        await _repository.DeleteAsync(task, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
