using MediatR;
using TodoCharacters.Application.Common;
using TodoCharacters.Application.Tasks.Dtos;

namespace TodoCharacters.Application.Tasks.Commands;

public sealed class DeleteTaskCommand : IRequest<Result>
{
    public Guid Id { get; init; }
}
