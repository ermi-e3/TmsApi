namespace TmsApi.Application.Common.Exceptions;

public sealed class NotFoundException : Exception
{
    public NotFoundException(string detail)
        : base(detail) { }

    public int StatusCode => 404;

    public string Title => "Resource not found";

    public string Type => "https://tms.local/errors/not_found";
}
