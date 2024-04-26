namespace NetGL;

public readonly struct Result<TValue, TError> {
    private readonly TValue value = default!;
    private readonly TError error = default!;
    private readonly bool success;

    private Result(TValue value) {
        this.success = true;
        this.value   = value;
    }

    private Result(TError error) {
        this.success = false;
        this.error   = error;
    }

    public static implicit operator Result<TValue, TError>(TValue value)
        => new Result<TValue, TError>(value);

    public static implicit operator Result<TValue, TError>(TError error)
        => new Result<TValue, TError>(error);

    public static implicit operator bool(Result<TValue, TError> result)
        => result.success;

    public static explicit operator TValue(Result<TValue, TError> result)
        => result.success
            ? result.value
            : throw new InvalidOperationException(
                                                  result.error != null
                                                      ? result.error.ToString()
                                                      : $"Result<{nameof(TValue)}, {nameof(TError)}> is in a failure state."
                                                 );

    public static explicit operator TError(Result<TValue, TError> result)
        => result.success
            ? throw new InvalidOperationException("Result<TValue, TError> is in a success state.")
            : result.error;
}

public readonly struct Result<T> {
    private readonly T value = default!;
    private readonly bool success = false;

    private Result(T value) {
        this.success = true;
        this.value   = value;
    }

    private Result(bool success) {
        if (success) throw new ArgumentException("Cannot convert a boolean to a Result<T> without a value.");
        this.success = success;
    }

    public static implicit operator Result<T>(T value) => new(value);

    public static implicit operator Result<T>(bool success)
        => new Result<T>(success);

    public static implicit operator bool(Result<T> result)
        => result.success;

    public static explicit operator T(Result<T> result)
        => result.success ? result.value : throw new InvalidOperationException("Result<T> is in a failure state.");
}