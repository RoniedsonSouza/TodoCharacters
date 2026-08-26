using TodoCharacters.Application.Tasks.Dtos;
using TodoCharacters.Domain.Entities;
using TodoTaskStatus = TodoCharacters.Domain.Enums.TaskStatus;

namespace TodoCharacters.Application.Tasks;

internal static class TaskMappings
{
    public static TaskDto ToDto(TaskItem task, bool includeSubtasks = true)
    {
        var subtasks = includeSubtasks
            ? task.Subtasks.Select(s => ToDto(s, false)).ToList()
            : new List<TaskDto>();

        return new TaskDto(
            task.Id,
            task.Title,
            task.Description,
            ToPortugueseStatus(task.Status),
            task.CreatedAt,
            task.CompletedAt,
            task.ParentTaskId,
            subtasks);
    }

    public static TasksGroupedDto ToGroupedDto(IEnumerable<TaskItem> rootTasks)
    {
        var ativas = new List<TaskDto>();
        var concluidas = new List<TaskDto>();

        foreach (var root in rootTasks.OrderBy(t => t.CreatedAt))
        {
            var dto = ToDto(root);
            if (root.Status == TodoTaskStatus.Completed)
                concluidas.Add(dto);
            else
                ativas.Add(dto);
        }

        return new TasksGroupedDto(ativas, concluidas);
    }

    public static bool TryParseStatus(string status, out TodoTaskStatus parsed)
    {
        parsed = status switch
        {
            "Não iniciado" => TodoTaskStatus.NotStarted,
            "Em progresso" => TodoTaskStatus.InProgress,
            "Concluído" => TodoTaskStatus.Completed,
            _ => default
        };

        return status is "Não iniciado" or "Em progresso" or "Concluído";
    }

    public static string ToPortugueseStatus(TodoTaskStatus status) => status switch
    {
        TodoTaskStatus.NotStarted => "Não iniciado",
        TodoTaskStatus.InProgress => "Em progresso",
        TodoTaskStatus.Completed => "Concluído",
        _ => throw new ArgumentOutOfRangeException(nameof(status))
    };
}
