using MediatR;
using TodoCharacters.Application.Abstractions;
using TodoCharacters.Application.Characters.Dtos;
using TodoCharacters.Application.Common;

namespace TodoCharacters.Application.Characters.Queries;

public sealed class GetCharactersQuery : IRequest<Result<CharacterPageDto>>
{
    public int Page { get; init; } = 1;
}

public sealed class GetCharactersQueryHandler : IRequestHandler<GetCharactersQuery, Result<CharacterPageDto>>
{
    private readonly ICharacterApiClient _client;

    public GetCharactersQueryHandler(ICharacterApiClient client)
    {
        _client = client;
    }

    public async Task<Result<CharacterPageDto>> Handle(GetCharactersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var page = await _client.GetCharactersAsync(request.Page, cancellationToken);
            return Result<CharacterPageDto>.Success(page);
        }
        catch (ExternalApiException ex)
        {
            return Result<CharacterPageDto>.Failure(new Error(
                ex.Code,
                ex.Message,
                ErrorType.ExternalService));
        }
    }
}

public sealed class GetCharacterByIdQuery : IRequest<Result<CharacterDetailDto>>
{
    public int Id { get; init; }
}

public sealed class GetCharacterByIdQueryHandler : IRequestHandler<GetCharacterByIdQuery, Result<CharacterDetailDto>>
{
    private readonly ICharacterApiClient _client;

    public GetCharacterByIdQueryHandler(ICharacterApiClient client)
    {
        _client = client;
    }

    public async Task<Result<CharacterDetailDto>> Handle(GetCharacterByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var character = await _client.GetCharacterByIdAsync(request.Id, cancellationToken);
            if (character is null)
            {
                return Result<CharacterDetailDto>.Failure(new Error(
                    "PERSONAGEM_NAO_ENCONTRADO",
                    "Personagem não encontrado.",
                    ErrorType.NotFound));
            }

            return Result<CharacterDetailDto>.Success(character);
        }
        catch (ExternalApiException ex)
        {
            return Result<CharacterDetailDto>.Failure(new Error(
                ex.Code,
                ex.Message,
                ErrorType.ExternalService));
        }
    }
}

public sealed class ExternalApiException : Exception
{
    public string Code { get; }

    public ExternalApiException(string code, string message) : base(message)
    {
        Code = code;
    }
}
