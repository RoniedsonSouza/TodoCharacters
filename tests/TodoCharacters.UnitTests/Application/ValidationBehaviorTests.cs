using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TodoCharacters.Application;
using TodoCharacters.Application.Common;
using TodoCharacters.Application.Tasks.Commands;
using TodoCharacters.Application.Tasks.Dtos;

namespace TodoCharacters.UnitTests.Application;

public class ValidationBehaviorTests
{
    [Fact]
    public async Task Validation_failure_should_return_Result_without_exception()
    {
        var services = new ServiceCollection();
        services.AddApplication();
        services.AddLogging();
        var provider = services.BuildServiceProvider();

        var mediator = provider.GetRequiredService<IMediator>();

        Exception? thrown = null;
        Result<TaskDto>? result = null;
        try
        {
            result = await mediator.Send(new CreateTaskCommand { Titulo = "ab" });
        }
        catch (Exception ex)
        {
            thrown = ex;
        }

        thrown.Should().BeNull();
        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorType.Validation);
    }
}
