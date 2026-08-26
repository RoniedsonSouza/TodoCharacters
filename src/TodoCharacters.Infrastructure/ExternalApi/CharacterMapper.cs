using TodoCharacters.Application.Characters.Dtos;

namespace TodoCharacters.Infrastructure.ExternalApi;

internal static class CharacterMapper
{
    public static CharacterPageDto ToPageDto(RickCharacterPageResponse response, int page)
    {
        var items = response.Results
            .Select(c => new CharacterSummaryDto(c.Id, c.Name, c.Status, c.Species, c.Image))
            .ToList();

        return new CharacterPageDto(
            items,
            page,
            response.Info.Pages,
            response.Info.Count,
            !string.IsNullOrEmpty(response.Info.Next),
            !string.IsNullOrEmpty(response.Info.Prev));
    }

    public static CharacterDetailDto ToDetailDto(RickCharacterDetailResponse response) =>
        new(
            response.Id,
            response.Name,
            response.Status,
            response.Species,
            response.Image,
            NormalizeOptional(response.Gender),
            NormalizeOptional(response.Origin.Name),
            NormalizeOptional(response.Location.Name),
            response.Episode.Count);

    internal static string NormalizeOptional(string value) =>
        string.IsNullOrWhiteSpace(value) || value.Equals("unknown", StringComparison.OrdinalIgnoreCase)
            ? "unknown"
            : value;
}
