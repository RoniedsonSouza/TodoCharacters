using FluentValidation;
using MediatR;
using TodoCharacters.Application.Common;
using TodoCharacters.Application.Tasks.Dtos;

namespace TodoCharacters.Application.Tasks.Commands;

public sealed class CreateTaskCommand : IRequest<Result<TaskDto>>
{
    public string Titulo { get; init; } = string.Empty;
    public string? Descricao { get; init; }
    public Guid? TarefaPaiId { get; init; }
}

public sealed class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(x => x.Titulo)
            .NotEmpty().WithMessage("Título é obrigatório.")
            .MinimumLength(3).WithMessage("Título deve ter no mínimo 3 caracteres.")
            .MaximumLength(120).WithMessage("Título deve ter no máximo 120 caracteres.");

        RuleFor(x => x.Descricao)
            .MaximumLength(1000).WithMessage("Descrição deve ter no máximo 1.000 caracteres.")
            .When(x => x.Descricao is not null);
    }
}
