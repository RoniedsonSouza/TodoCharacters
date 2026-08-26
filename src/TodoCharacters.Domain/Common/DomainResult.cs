namespace TodoCharacters.Domain.Common;

public sealed record DomainResult(bool IsSuccess, string? ErrorCode = null, string? Message = null)
{
    public static DomainResult Ok() => new(true);

    public static DomainResult Fail(string errorCode, string message) =>
        new(false, errorCode, message);
}
