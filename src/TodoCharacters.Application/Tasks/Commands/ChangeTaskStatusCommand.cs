using FluentValidation;
using MediatR;
using TodoCharacters.Application.Common;
using TodoCharacters.Application.Tasks.Dtos;

namespace TodoCharacters.Application.Tasks.Commands;

public sealed class ChangeTaskStatusCommand : IRequest<Result<TaskDto>>
{
    public Guid Id { get; init; }
    public string Status { get; init; } = string.Empty;
}

public sealed class ChangeTaskStatusCommandValidator : AbstractValidator<ChangeTaskStatusCommand>
{
    public ChangeTaskStatusCommandValidator()
    {
        RuleFor(x => x.Status)
            .Must(s => s is "Não iniciado" or "Em progresso" or "Concluído")
            .WithMessage("Status inválido. Use: Não iniciado, Em progresso ou Concluído.");
    }
}
