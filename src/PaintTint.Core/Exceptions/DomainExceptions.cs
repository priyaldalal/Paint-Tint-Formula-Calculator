namespace PaintTint.Core.Exceptions;

/// <summary>
/// Exception thrown when a paint formulation fails validation (e.g., exceeds base maximum tint percentage).
/// </summary>
public class TintValidationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TintValidationException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message describing why formulation validation failed.</param>
    public TintValidationException(string message) : base(message)
    {
    }
}

/// <summary>
/// Exception thrown when a requested domain entity (such as Shade or Base) cannot be found.
/// </summary>
public class NotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message describing the missing resource.</param>
    public NotFoundException(string message) : base(message)
    {
    }
}

