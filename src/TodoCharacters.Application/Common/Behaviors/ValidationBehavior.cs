using FluentValidation;
using MediatR;
using TodoCharacters.Application.Common.Interfaces;

namespace TodoCharacters.Application.Common.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly IResultFactory _resultFactory;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators, IResultFactory resultFactory)
    {
        _validators = validators;
        _resultFactory = resultFactory;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);
        var failures = (await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken))))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count == 0)
            return await next();

        var message = string.Join("; ", failures.Select(f => f.ErrorMessage));
        var error = new Error("VALIDATION_ERROR", message, ErrorType.Validation);
        return (TResponse)_resultFactory.CreateFailure(typeof(TResponse), error);
    }
}
