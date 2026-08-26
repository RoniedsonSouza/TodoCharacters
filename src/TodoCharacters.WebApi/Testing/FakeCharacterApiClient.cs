using TodoCharacters.Application.Abstractions;
using TodoCharacters.Application.Characters.Dtos;

namespace TodoCharacters.WebApi.Testing;

public sealed class FakeCharacterApiClient : ICharacterApiClient
{
    public Task<CharacterPageDto> GetCharactersAsync(int page, CancellationToken cancellationToken = default)
    {
        var items = page == 1
            ? new List<CharacterSummaryDto>
            {
                new(1, "Rick Sanchez", "Alive", "Human", "https://example.com/rick.png"),
                new(2, "Morty Smith", "Alive", "Human", "https://example.com/morty.png"),
            }
            : new List<CharacterSummaryDto>
            {
                new(3, "Summer Smith", "Alive", "Human", "https://example.com/summer.png"),
            };

        return Task.FromResult(new CharacterPageDto(items, page, 2, 3, page < 2, page > 1));
    }

    public Task<CharacterDetailDto?> GetCharacterByIdAsync(int id, CancellationToken cancellationToken = default) =>
        Task.FromResult<CharacterDetailDto?>(new CharacterDetailDto(
            id, "Rick Sanchez", "Alive", "Human", "https://example.com/rick.png",
            "Male", "Earth", "Citadel", 51));
}
