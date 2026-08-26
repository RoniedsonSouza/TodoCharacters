using TodoCharacters.Application.Characters.Dtos;

namespace TodoCharacters.Application.Abstractions;

public interface ICharacterApiClient
{
    Task<CharacterPageDto> GetCharactersAsync(int page, CancellationToken cancellationToken = default);
    Task<CharacterDetailDto?> GetCharacterByIdAsync(int id, CancellationToken cancellationToken = default);
}
