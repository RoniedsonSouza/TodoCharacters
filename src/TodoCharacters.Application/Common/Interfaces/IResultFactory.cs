using System.Reflection;
using TodoCharacters.Application.Common;

namespace TodoCharacters.Application.Common.Interfaces;

public interface IResultFactory
{
    object CreateFailure(Type responseType, Error error);
}

public sealed class ResultFactory : IResultFactory
{
    public object CreateFailure(Type responseType, Error error)
    {
        if (!responseType.IsGenericType || responseType.GetGenericTypeDefinition() != typeof(Result<>))
            throw new InvalidOperationException($"Type {responseType.Name} is not Result<T>.");

        var valueType = responseType.GetGenericArguments()[0];
        var failureMethod = typeof(Result<>)
            .MakeGenericType(valueType)
            .GetMethod(nameof(Result<object>.Failure), BindingFlags.Public | BindingFlags.Static);

        return failureMethod!.Invoke(null, new object[] { error })!;
    }
}
