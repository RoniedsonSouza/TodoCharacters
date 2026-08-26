namespace TodoCharacters.Application.Characters.Dtos;

public sealed record CharacterSummaryDto(
    int Id,
    string Nome,
    string Status,
    string Especie,
    string ImagemUrl);

public sealed record CharacterDetailDto(
    int Id,
    string Nome,
    string Status,
    string Especie,
    string ImagemUrl,
    string Genero,
    string Origem,
    string Localizacao,
    int QuantidadeEpisodios);

public sealed record CharacterPageDto(
    IReadOnlyList<CharacterSummaryDto> Items,
    int Page,
    int TotalPages,
    int TotalCount,
    bool HasNext,
    bool HasPrev);
