using FluentValidation;
using MediatR;
using TodoCharacters.Application.Common;
using TodoCharacters.Application.Tasks.Dtos;

namespace TodoCharacters.Application.Tasks.Commands;

public sealed class UpdateTaskCommand : IRequest<Result<TaskDto>>
{
    public Guid Id { get; init; }
    public string Titulo { get; init; } = string.Empty;
    public string? Descricao { get; init; }
}

public sealed class UpdateTaskCommandValidator : AbstractValidator<UpdateTaskCommand>
{
    public UpdateTaskCommandValidator()
    {
        RuleFor(x => x.Titulo)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(120);

        RuleFor(x => x.Descricao)
            .MaximumLength(1000)
            .When(x => x.Descricao is not null);
    }
}
