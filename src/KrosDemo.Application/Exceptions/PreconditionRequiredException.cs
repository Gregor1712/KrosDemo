namespace KrosDemo.Application.Exceptions;

public class PreconditionRequiredException : Exception
{
    public PreconditionRequiredException(string message) : base(message) { }

    public static PreconditionRequiredException MissingIfMatch() =>
        new("Provide the resource's current ETag in the If-Match header to perform this conditional operation.");
}