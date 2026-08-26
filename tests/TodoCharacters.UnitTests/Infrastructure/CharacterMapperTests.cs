using FluentAssertions;
using TodoCharacters.Infrastructure.ExternalApi;

namespace TodoCharacters.UnitTests.Infrastructure;

public class CharacterMapperTests
{
    [Fact]
    public void Should_map_episode_count_from_array_length()
    {
        var response = new RickCharacterDetailResponse
        {
            Id = 1,
            Name = "Rick",
            Status = "Alive",
            Species = "Human",
            Gender = "Male",
            Image = "http://img",
            Origin = new RickNamedResource { Name = "Earth" },
            Location = new RickNamedResource { Name = "Citadel" },
            Episode = ["ep1", "ep2", "ep3"]
        };

        var dto = CharacterMapper.ToDetailDto(response);

        dto.QuantidadeEpisodios.Should().Be(3);
    }

    [Fact]
    public void Should_map_unknown_values()
    {
        var response = new RickCharacterDetailResponse
        {
            Id = 2,
            Name = "Unknown",
            Gender = "unknown",
            Origin = new RickNamedResource { Name = "unknown" },
            Location = new RickNamedResource { Name = "unknown" },
            Episode = []
        };

        var dto = CharacterMapper.ToDetailDto(response);

        dto.Genero.Should().Be("unknown");
        dto.Origem.Should().Be("unknown");
        dto.Localizacao.Should().Be("unknown");
    }

    [Fact]
    public void Should_map_empty_string_values()
    {
        var response = new RickCharacterDetailResponse
        {
            Id = 3,
            Name = "Test",
            Type = "",
            Origin = new RickNamedResource { Name = "", Url = "" },
            Episode = []
        };

        CharacterMapper.NormalizeOptional("").Should().Be("unknown");
        CharacterMapper.NormalizeOptional("Earth").Should().Be("Earth");
    }
}
