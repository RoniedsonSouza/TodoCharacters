namespace TodoCharacters.Infrastructure.ExternalApi;

public sealed class RickAndMortyOptions
{
    public const string SectionName = "RickAndMortyApi";

    public string BaseUrl { get; set; } = "https://rickandmortyapi.com/api/";

    public int TimeoutSeconds { get; set; } = 10;
}
