namespace NetGL;

public readonly struct Result<T> where T: class {
    public readonly T value;
    private readonly bool succeeded;

    public Result(bool result, in T value) {
        this.succeeded = result;
        this.value = value;
    }

    public Result() {
        succeeded = false;
        value = default!;
    }

    public static implicit operator Result<T>(in T value) => new Result<T>(true, value);

    public static implicit operator Result<T>(in bool value) {
        if (value) throw new ArgumentException("Cannot convert a boolean to a Result<T> without a value.");
        return new Result<T>();
    }

    public static implicit operator T(in Result<T> r) => r.succeeded ? r.value : throw new NullReferenceException();
    public static bool operator true(in Result<T> r) => r.succeeded;
    public static bool operator false(in Result<T> r) => !r.succeeded;
    public static implicit operator bool(in Result<T> r) => r.succeeded;

    public (bool result, T value) result => (succeeded, value);

    public static Result<T> success(in T value) => new(true, value);
    public static Result<T> failure() => new Result<T>();
}