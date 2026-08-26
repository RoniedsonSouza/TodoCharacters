using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TodoCharacters.Application.Abstractions;
using TodoCharacters.Infrastructure.ExternalApi;
using TodoCharacters.Infrastructure.Persistence;
using TodoCharacters.Infrastructure.Persistence.Repositories;

namespace TodoCharacters.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RickAndMortyOptions>(configuration.GetSection(RickAndMortyOptions.SectionName));

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddHttpClient<ICharacterApiClient, RickAndMortyClient>();

        return services;
    }
}
