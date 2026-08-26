namespace TodoCharacters.Application.Tasks.Dtos;

public sealed record TaskDto(
    Guid Id,
    string Titulo,
    string? Descricao,
    string Status,
    DateTimeOffset CriadaEm,
    DateTimeOffset? ConcluidaEm,
    Guid? TarefaPaiId,
    IReadOnlyList<TaskDto> Subtarefas);

public sealed record TasksGroupedDto(
    IReadOnlyList<TaskDto> Ativas,
    IReadOnlyList<TaskDto> Concluidas);

public sealed record CreateTaskRequest(string Titulo, string? Descricao, Guid? TarefaPaiId);

public sealed record UpdateTaskRequest(string Titulo, string? Descricao);

public sealed record ChangeTaskStatusRequest(string Status);
