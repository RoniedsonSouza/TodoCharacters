using TodoCharacters.Application.Common;

namespace TodoCharacters.WebApi.Mapping;

public static class ResultToHttpMapper
{
    public static IResult ToHttpResult(Result result) =>
        result.IsSuccess
            ? Results.NoContent()
            : ToErrorResult(result.Error!);

    public static IResult ToHttpResult<T>(Result<T> result) =>
        result.IsSuccess
            ? Results.Ok(result.Value)
            : ToErrorResult(result.Error!);

    public static IResult ToCreatedResult<T>(Result<T> result, string location) =>
        result.IsSuccess
            ? Results.Created(location, result.Value)
            : ToErrorResult(result.Error!);

    private static IResult ToErrorResult(Error error) =>
        error.Type switch
        {
            ErrorType.Validation => Results.BadRequest(new { code = error.Code, message = error.Message }),
            ErrorType.NotFound => Results.NotFound(new { code = error.Code, message = error.Message }),
            ErrorType.Conflict => Results.Conflict(new { code = error.Code, message = error.Message }),
            ErrorType.ExternalService => Results.Json(new { code = error.Code, message = error.Message }, statusCode: 503),
            _ => Results.Problem(error.Message)
        };
}
