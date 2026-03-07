namespace Core.CrossCuttingConcerns.Exceptions.Types;

public class ValidationException : Exception
{
    public ValidationException() : base()
    {
        ErrorMessages = Array.Empty<ValidationExceptionModel>();
    }

    public ValidationException(string? message) : base(message)
    {
        ErrorMessages = Array.Empty<ValidationExceptionModel>();
    }

    public ValidationException(string? message, Exception? innerException) : base(message, innerException)
    {
        ErrorMessages = Array.Empty<ValidationExceptionModel>();
    }

    public ValidationException(IEnumerable<ValidationExceptionModel> errors) : base(BuildErrorMessage(errors))
    {
        ErrorMessages = errors;
    }

    private static string BuildErrorMessage(IEnumerable<ValidationExceptionModel> errors)
    {
        IEnumerable<string> arr = errors.Select(x => $"{Environment.NewLine} -- {x.PropertyName}: {string.Join(Environment.NewLine, values: x.ErrorMessages ?? Array.Empty<string>())}");
        
        return $"Validation failed: {string.Join(string.Empty, arr)}";
    }

    public IEnumerable<ValidationExceptionModel>? ErrorMessages { get; }
}

public class ValidationExceptionModel
{
    public string? PropertyName { get; set; }
    public IEnumerable<string>? ErrorMessages { get; set; }
}
