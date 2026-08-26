using TodoCharacters.Domain.Common;
using TodoTaskStatus = TodoCharacters.Domain.Enums.TaskStatus;

namespace TodoCharacters.Domain.Entities;

public class TaskItem
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public TodoTaskStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public Guid? ParentTaskId { get; private set; }
    public TaskItem? ParentTask { get; private set; }
    public ICollection<TaskItem> Subtasks { get; private set; } = new List<TaskItem>();

    private TaskItem()
    {
    }

    public static TaskItem CreateRoot(string title, string? description, DateTimeOffset createdAt)
    {
        ValidateTitle(title);
        ValidateDescription(description);

        return new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = title.Trim(),
            Description = description?.Trim(),
            Status = TodoTaskStatus.NotStarted,
            CreatedAt = createdAt
        };
    }

    public static DomainResult CreateSubtask(
        TaskItem parent,
        string title,
        string? description,
        DateTimeOffset createdAt,
        out TaskItem? subtask)
    {
        subtask = null;

        if (parent.ParentTaskId.HasValue)
        {
            return DomainResult.Fail(
                "SUBTAREFA_PROFUNDIDADE_MAXIMA",
                "Uma subtarefa não pode possuir outra subtarefa.");
        }

        ValidateTitle(title);
        ValidateDescription(description);

        subtask = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = title.Trim(),
            Description = description?.Trim(),
            Status = TodoTaskStatus.NotStarted,
            CreatedAt = createdAt,
            ParentTaskId = parent.Id,
            ParentTask = parent
        };

        parent.Subtasks.Add(subtask);
        return DomainResult.Ok();
    }

    public DomainResult Update(string title, string? description)
    {
        ValidateTitle(title);
        ValidateDescription(description);

        Title = title.Trim();
        Description = description?.Trim();
        return DomainResult.Ok();
    }

    public DomainResult ChangeStatus(TodoTaskStatus newStatus, IEnumerable<TaskItem> subtasks)
    {
        if (newStatus == TodoTaskStatus.Completed && ParentTaskId is null)
        {
            var pending = subtasks.Where(s => s.Status != TodoTaskStatus.Completed).ToList();
            if (pending.Count > 0)
            {
                return DomainResult.Fail(
                    "TAREFA_PAI_SUBTAREFAS_PENDENTES",
                    "Conclua todas as subtarefas antes de finalizar esta tarefa.");
            }
        }

        Status = newStatus;
        CompletedAt = newStatus == TodoTaskStatus.Completed
            ? DateTimeOffset.UtcNow
            : null;

        return DomainResult.Ok();
    }

    public void ReopenFromCompleted()
    {
        if (Status == TodoTaskStatus.Completed)
        {
            Status = TodoTaskStatus.InProgress;
            CompletedAt = null;
        }
    }

    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Título é obrigatório.", nameof(title));

        var trimmed = title.Trim();
        if (trimmed.Length < 3 || trimmed.Length > 120)
            throw new ArgumentException("Título deve ter entre 3 e 120 caracteres.", nameof(title));
    }

    private static void ValidateDescription(string? description)
    {
        if (description is not null && description.Length > 1000)
            throw new ArgumentException("Descrição deve ter no máximo 1.000 caracteres.", nameof(description));
    }
}
