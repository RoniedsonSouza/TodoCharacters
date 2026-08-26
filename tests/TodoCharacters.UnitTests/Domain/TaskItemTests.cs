using FluentAssertions;
using TodoCharacters.Domain.Entities;
using TodoTaskStatus = TodoCharacters.Domain.Enums.TaskStatus;

namespace TodoCharacters.UnitTests.Domain;

public class TaskItemTests
{
    [Fact]
    public void CreateRoot_should_start_as_NaoIniciado_with_utc_timestamp()
    {
        var task = TaskItem.CreateRoot("Comprar pão", "Desc", DateTimeOffset.UtcNow);

        task.Status.Should().Be(TodoTaskStatus.NotStarted);
        task.CreatedAt.Offset.Should().Be(TimeSpan.Zero);
        task.CompletedAt.Should().BeNull();
    }

    [Fact]
    public void ChangeStatus_to_Completed_should_set_CompletedAt()
    {
        var task = TaskItem.CreateRoot("Tarefa", null, DateTimeOffset.UtcNow);

        var result = task.ChangeStatus(TodoTaskStatus.Completed, Array.Empty<TaskItem>());

        result.IsSuccess.Should().BeTrue();
        task.CompletedAt.Should().NotBeNull();
        task.CompletedAt!.Value.Offset.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void ChangeStatus_from_Completed_should_clear_CompletedAt()
    {
        var task = TaskItem.CreateRoot("Tarefa", null, DateTimeOffset.UtcNow);
        task.ChangeStatus(TodoTaskStatus.Completed, Array.Empty<TaskItem>());

        task.ChangeStatus(TodoTaskStatus.InProgress, Array.Empty<TaskItem>());

        task.CompletedAt.Should().BeNull();
    }

    [Fact]
    public void Parent_cannot_complete_with_pending_subtask()
    {
        var parent = TaskItem.CreateRoot("Pai", null, DateTimeOffset.UtcNow);
        TaskItem.CreateSubtask(parent, "Filho", null, DateTimeOffset.UtcNow, out var child);

        var result = parent.ChangeStatus(TodoTaskStatus.Completed, parent.Subtasks);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("TAREFA_PAI_SUBTAREFAS_PENDENTES");
    }

    [Fact]
    public void Subtask_in_subtask_should_return_conflict()
    {
        var root = TaskItem.CreateRoot("Raiz", null, DateTimeOffset.UtcNow);
        TaskItem.CreateSubtask(root, "Filho", null, DateTimeOffset.UtcNow, out var child);

        var result = TaskItem.CreateSubtask(child!, "Neto", null, DateTimeOffset.UtcNow, out _);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("SUBTAREFA_PROFUNDIDADE_MAXIMA");
    }

    [Fact]
    public void ReopenFromCompleted_should_move_to_InProgress()
    {
        var task = TaskItem.CreateRoot("Tarefa", null, DateTimeOffset.UtcNow);
        task.ChangeStatus(TodoTaskStatus.Completed, Array.Empty<TaskItem>());

        task.ReopenFromCompleted();

        task.Status.Should().Be(TodoTaskStatus.InProgress);
        task.CompletedAt.Should().BeNull();
    }
}
