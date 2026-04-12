namespace LD.Messaging.Domain;

public static class CheckedExtensions
{
    /// <summary>
    /// Executes a side-effect on the value if the check succeeded, then returns the
    /// original check unchanged. Useful for logging mid-pipeline without breaking the chain.
    /// </summary>
    public static Checked<T> Tap<T>(this Checked<T> source, Action<T> action)
    {
        if (source.IsSuccess)
        {
            action(source.Value);
        }

        return source;
    }

    /// <summary>
    /// Executes a side-effect on the error message if the check failed, then returns
    /// the original check unchanged. Useful for logging failures mid-pipeline.
    /// </summary>
    public static Checked<T> TapError<T>(this Checked<T> source, Action<string> action)
    {
        if (source.IsFailure)
        {
            action(source.Error);
        }

        return source;
    }

    /// <summary>
    /// Returns the value if successful, or the provided default if failed.
    /// </summary>
    public static T GetValueOrDefault<T>(this Checked<T> source, T defaultValue)
        => source.IsSuccess ? source.Value : defaultValue;
}
