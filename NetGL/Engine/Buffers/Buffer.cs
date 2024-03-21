namespace NetGL;

using OpenTK.Graphics.OpenGL4;
using System.Runtime.CompilerServices;

public interface IBindable: IEquatable<IBindable> {
    int handle { get; }
    void bind();

    bool IEquatable<IBindable>.Equals(IBindable? other) => other?.handle == handle;
}

public interface IBuffer: IBindable {
    int length { get; }
    Type item_type { get; }
    int item_size{ get; }
    int total_size { get; }
    void upload();
    Buffer.Status status { get; }
}

public abstract class Buffer: IBuffer {
    public enum Status {
        Empty,
        Filled,
        Uploaded,
        Modified
    }

    public int handle { get; protected set; }

    public abstract int length { get; }
    public abstract int item_size { get; }
    public abstract Type item_type { get; }
    public abstract int total_size { get; }

    public abstract void upload();
    public abstract void bind();

    public override string ToString() => $"{GetType().get_type_name(false)} (type={item_type.get_type_name()}, length={length:N0}, size={total_size:N0}, status={status})";

    public Status status {
        get;
        protected set;
    }
}

public abstract class Buffer<T>: Buffer, IDisposable where T: unmanaged {
    protected readonly NativeArray<T> buffer;
    private readonly BufferTarget target;

    ~Buffer() {
        buffer.Dispose();
    }

    protected Buffer(BufferTarget target, in ReadOnlySpan<T> items) {
        this.target = target;
        this.handle = 0;
        if(items.Length == 0) Error.empty_array<T>(nameof(items));
        this.buffer = new NativeArray<T>(items);
    }

    protected Buffer(BufferTarget target, int count) {
        this.target = target;
        this.handle = 0;
        this.buffer = new NativeArray<T>(count);
    }

    public override Type item_type => typeof(T);

    public T this[int index] {
        get {
            if (index < 0 || index >= buffer.length)
                throw new IndexOutOfRangeException($"Index out of range: {index}!");

            return buffer[index];
        }
        set {
            if (index < 0 || index >= buffer.length)
                throw new IndexOutOfRangeException($"Index out of range: {index}!");

            buffer[index] = value;
        }
    }

    public ArrayView<T> this[System.Range range]
        => buffer.get_view(range);
    
    public void resize(int new_size) {
        if (new_size < 0)
            throw new ArgumentOutOfRangeException(nameof(new_size));

        if (new_size == buffer.length)
            return;

        Console.WriteLine($"Buffer.resize: {buffer.length} -> {new_size}");

        buffer.resize(new_size);
    }

    public void insert(int index, in T[] items) {
        buffer.insert(items, index);
    }

    public void insert(int index, in T item) {
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (index + 1 > buffer.length)
            throw new ArgumentOutOfRangeException(nameof(index));

        buffer[index] = item;
    }

    public int append(in T[] items) {
        var position = buffer.length;
        resize(buffer.length + items.Length);
        insert(position, items);
        return position;
    }

    public int append(in T item) {
        int position = buffer.length;
        resize(buffer.length + 1);
        insert(position, item);
        return position;
    }

    public override int length => buffer.length;
    public override int item_size => Unsafe.SizeOf<T>();
    public override int total_size => item_size * length;

    public override void bind() {
        Console.WriteLine("Binding " + ToString());

        if (handle == 0)
            throw new NotSupportedException("no handle has been allocated yet!");

        GL.BindBuffer(target, handle);
    }

    public override void upload() {
        if (handle == 0) {
            handle = GL.GenBuffer();
        }

        bind();
        GL.BufferData(target, buffer.length * item_size, buffer.get_pointer(), BufferUsageHint.StaticDraw);

        status = Status.Uploaded;
    }

    public void Dispose() {
        buffer.Dispose();
        GC.SuppressFinalize(this);
    }

    protected ArrayView<T> get_view() => buffer.get_view<T>();
    protected ArrayView<T> get_view(System.Range range) => buffer.get_view<T>(range);
    protected ArrayView<V> get_view<V>() where V: unmanaged => buffer.get_view<V>();
    protected ArrayView<V> get_view<V>(string field_name) where V : unmanaged => buffer.get_view<V>(field_name);
}