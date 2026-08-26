namespace TodoCharacters.Application.Common;

public sealed record Error(string Code, string Message, ErrorType Type);

public enum ErrorType
{
    Validation = 400,
    NotFound = 404,
    Conflict = 409,
    ExternalService = 503
}
