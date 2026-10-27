namespace WoWRandomizer.Infrastructure;

/// <summary>Thrown when the combo data file is missing or its content is invalid.</summary>
public sealed class ComboDataException : Exception
{
    public ComboDataException(string message) : base(message)
    {
    }

    public ComboDataException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
