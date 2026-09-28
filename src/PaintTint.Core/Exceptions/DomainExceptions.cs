namespace PaintTint.Core.Exceptions;

public class TintValidationException : Exception
{
    public TintValidationException(string message) : base(message)
    {
    }
}

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }
}
