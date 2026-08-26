using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TodoCharacters.Domain.Entities;
using TodoCharacters.Infrastructure.Persistence;
using TodoCharacters.Infrastructure.Persistence.Repositories;

namespace TodoCharacters.IntegrationTests.Persistence;

public class DeleteParentTaskCascadeTests
{
    private static bool CanConnectToSqlServer()
    {
        try
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer("Server=localhost,1433;Database=TodoCharacters_Test;User Id=sa;Password=TodoCharacters@123;TrustServerCertificate=True;Connect Timeout=3")
                .Options;
            using var context = new AppDbContext(options);
            context.Database.EnsureCreated();
            return context.Database.CanConnect();
        }
        catch
        {
            return false;
        }
    }

    [Fact]
    public async Task Delete_parent_should_remove_subtasks_in_same_transaction()
    {
        if (!CanConnectToSqlServer())
        {
            return; // Skip when SQL Server container is not running
        }

        var connectionString = "Server=localhost,1433;Database=TodoCharacters_Test;User Id=sa;Password=TodoCharacters@123;TrustServerCertificate=True";
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        await using var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var repository = new TaskRepository(context);
        var parent = TaskItem.CreateRoot("Pai cascade", null, DateTimeOffset.UtcNow);
        TaskItem.CreateSubtask(parent, "Filho cascade", null, DateTimeOffset.UtcNow, out var child);

        await repository.AddAsync(parent, CancellationToken.None);
        await repository.AddAsync(child!, CancellationToken.None);
        await repository.SaveChangesAsync(CancellationToken.None);

        var childId = child!.Id;
        await repository.DeleteAsync(parent, CancellationToken.None);
        await repository.SaveChangesAsync(CancellationToken.None);

        var remaining = await context.Tasks.CountAsync();
        remaining.Should().Be(0);
        (await context.Tasks.FindAsync(childId)).Should().BeNull();
    }
}
