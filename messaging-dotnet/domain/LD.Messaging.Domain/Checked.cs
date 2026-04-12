namespace LD.Messaging.Domain;

/// <summary>
/// A monad that represents the result of a checked operation — either a successful
/// value (Ok) or a descriptive failure (Fail). Chain operations with Bind/Map to
/// build railway-oriented validation pipelines where the first failure short-circuits
/// the remainder of the chain.
/// </summary>
public sealed class Checked<T>
{
    private readonly T? _value;
    private readonly string? _error;

    private Checked(T value)
    {
        _value = value;
        IsSuccess = true;
    }

    private Checked(string error)
    {
        _error = error;
        IsSuccess = false;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    public T Value =>
        IsSuccess
            ? _value!
            : throw new InvalidOperationException(
                $"Cannot access Value on a failed Checked<{typeof(T).Name}>: {_error}");

    public string Error =>
        IsFailure
            ? _error!
            : throw new InvalidOperationException(
                $"Cannot access Error on a successful Checked<{typeof(T).Name}>");

    /// <summary>Creates a successful check wrapping the given value.</summary>
    public static Checked<T> Ok(T value) => new(value);

    /// <summary>Creates a failed check with the given error description.</summary>
    public static Checked<T> Fail(string error) => new(error);

    /// <summary>
    /// If this check succeeded, runs <paramref name="next"/> on the value and returns
    /// its result. If this check already failed, propagates the failure without calling
    /// <paramref name="next"/>.
    /// </summary>
    public Checked<TNext> Bind<TNext>(Func<T, Checked<TNext>> next)
        => IsSuccess ? next(_value!) : Checked<TNext>.Fail(_error!);

    /// <summary>
    /// If this check succeeded, transforms the value using <paramref name="transform"/>
    /// and wraps it in a new Ok. Failures propagate unchanged.
    /// </summary>
    public Checked<TNext> Map<TNext>(Func<T, TNext> transform)
        => IsSuccess ? Checked<TNext>.Ok(transform(_value!)) : Checked<TNext>.Fail(_error!);

    /// <summary>Executes one of two actions depending on success or failure.</summary>
    public void Match(Action<T> onSuccess, Action<string> onFailure)
    {
        if (IsSuccess) onSuccess(_value!);
        else onFailure(_error!);
    }

    /// <summary>Projects the check into a single result value.</summary>
    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onFailure)
        => IsSuccess ? onSuccess(_value!) : onFailure(_error!);

    public override string ToString()
        => IsSuccess ? $"Ok({_value})" : $"Fail({_error})";
}
