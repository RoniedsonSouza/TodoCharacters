using Microsoft.EntityFrameworkCore;
using TodoCharacters.Application.Abstractions;
using TodoCharacters.Domain.Entities;
using TodoCharacters.Infrastructure.Persistence;

namespace TodoCharacters.Infrastructure.Persistence.Repositories;

public sealed class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _context;

    public TaskRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<TaskItem?> GetByIdWithSubtasksAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Tasks
            .Include(t => t.Subtasks)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<IReadOnlyList<TaskItem>> GetAllWithSubtasksAsync(CancellationToken cancellationToken = default) =>
        await _context.Tasks
            .Include(t => t.Subtasks)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(TaskItem task, CancellationToken cancellationToken = default) =>
        await _context.Tasks.AddAsync(task, cancellationToken);

    public Task UpdateAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        _context.Tasks.Update(task);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        _context.Tasks.Remove(task);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
