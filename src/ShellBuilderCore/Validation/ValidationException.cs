namespace ShellBuilderCore.Validation;

public class ValidationException : Exception
{
    public ValidationErrorCode ErrorCode { get; private set; }
    
    public ValidationException(ValidationErrorCode errorCode, string? message = null) : base(message)
    {
        ErrorCode = errorCode;
    }
}