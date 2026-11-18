namespace Claims.Services;

/// <summary>
/// Thrown when a request breaks a business rule that can only be checked in the service layer
/// (for example, rules that need data from the database).
/// </summary>
public class ValidationException : Exception
{
    /// <summary>
    /// Creates the exception with a message that is safe to return to the client.
    /// </summary>
    public ValidationException(string message)
        : base(message)
    {
    }
}
