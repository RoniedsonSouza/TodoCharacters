using FluentAssertions;
using Moq;
using TodoCharacters.Application.Abstractions;
using TodoCharacters.Application.Common;
using TodoCharacters.Application.Tasks.Commands;
using TodoCharacters.Domain.Entities;

namespace TodoCharacters.UnitTests.Application;

public class CreateTaskCommandHandlerTests
{
    [Fact]
    public async Task Invalid_title_should_return_validation_failure_without_exception()
    {
        var repository = new Mock<ITaskRepository>();
        var handler = new CreateTaskCommandHandler(repository.Object);

        var result = await handler.Handle(new CreateTaskCommand { Titulo = "ab" }, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task Subtask_in_subtask_should_return_conflict()
    {
        var parent = TaskItem.CreateRoot("Raiz", null, DateTimeOffset.UtcNow);
        TaskItem.CreateSubtask(parent, "Filho", null, DateTimeOffset.UtcNow, out var child);

        var repository = new Mock<ITaskRepository>();
        repository.Setup(r => r.GetByIdAsync(parent.Id, It.IsAny<CancellationToken>())).ReturnsAsync(parent);
        repository.Setup(r => r.GetByIdAsync(child!.Id, It.IsAny<CancellationToken>())).ReturnsAsync(child);

        var handler = new CreateTaskCommandHandler(repository.Object);
        var result = await handler.Handle(new CreateTaskCommand
        {
            Titulo = "Neto",
            TarefaPaiId = child.Id
        }, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorType.Conflict);
    }
}

public class ChangeTaskStatusCommandHandlerTests
{
    [Fact]
    public async Task Deve_reverter_pai_para_em_progresso_quando_subtarefa_deixa_de_ser_concluida()
    {
        var parent = TaskItem.CreateRoot("Pai", null, DateTimeOffset.UtcNow);
        TaskItem.CreateSubtask(parent, "Filho", null, DateTimeOffset.UtcNow, out var child);
        child!.ChangeStatus(TodoCharacters.Domain.Enums.TaskStatus.Completed, Array.Empty<TaskItem>());
        parent.ChangeStatus(TodoCharacters.Domain.Enums.TaskStatus.Completed, parent.Subtasks);

        var repository = new Mock<ITaskRepository>();
        repository.Setup(r => r.GetByIdWithSubtasksAsync(child.Id, It.IsAny<CancellationToken>())).ReturnsAsync(child);
        repository.Setup(r => r.GetByIdWithSubtasksAsync(parent.Id, It.IsAny<CancellationToken>())).ReturnsAsync(parent);

        var handler = new ChangeTaskStatusCommandHandler(repository.Object);
        var result = await handler.Handle(new ChangeTaskStatusCommand
        {
            Id = child.Id,
            Status = "Em progresso"
        }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        parent.Status.Should().Be(TodoCharacters.Domain.Enums.TaskStatus.InProgress);
        parent.CompletedAt.Should().BeNull();
    }
}
