namespace NetGL;

using OpenTK.Graphics.OpenGL4;
using System.Runtime.CompilerServices;

public interface IBindable {
    int handle { get; }
    public void bind();
}

public interface IBindableIndexed {
    int handle { get; }
    int binding_point { get; }

    public void bind(int binding_point);
}

public interface IBuffer {
    int length { get; }
    public Type item_type { get; }
    public int item_size{ get; }
    public Buffer.Status status { get; }
    public int version { get; }
}

public abstract class Buffer: IBuffer {
    private int _length;
    public enum Status {
        Empty,
        Filled,
        Uploaded,
        Modified
    }

    public int handle { get; protected set; }

    public abstract int capacity { get; }

    public int length {
        get => _length;
        set {
            if (value > capacity)
                Error.index_out_of_range(value, capacity);
            _length = value;
        }
    }

    public abstract int item_size { get; }
    public abstract Type item_type { get; }

    public abstract void create();

    public abstract void update();

    public override string ToString() => $"{GetType().get_type_name(false)} (type={item_type.get_type_name()}, capacity={capacity:N0}, length={length:N0}, status={status})";

    public int version { get; protected set; }

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

    protected Buffer(BufferTarget target, ReadOnlySpan<T> items) {
        this.target = target;
        this.handle = 0;
        if(items.Length == 0) Error.empty_array<T>(nameof(items));
        this.buffer = new NativeArray<T>(items);
        length = items.Length;
    }

    protected Buffer(BufferTarget target, int capacity) {
        this.target = target;
        this.handle = 0;
        this.buffer = new NativeArray<T>(capacity);
        length = capacity;
    }

    protected Buffer(BufferTarget target, T data) {
        this.target = target;
        this.handle = 0;
        this.buffer = new NativeArray<T>(1);
        buffer[0] = data;
        length = 1;
    }

    public void set_data(ReadOnlySpan<T> items) {
        if(items.Length > capacity) Error.index_out_of_range(items.Length, capacity);
        buffer.insert(items, 0);
        length = items.Length;
    }

    public sealed override int capacity => buffer.length;

    public sealed override Type item_type => typeof(T);

    public ref T this[int index] {
        get {
            if (index < 0 || index >= length)
                throw new IndexOutOfRangeException($"Index out of range: {index}!");

            return ref buffer.by_ref(index);
        }
    }

    public void clear() {
        buffer.zero();
        length = 0;
    }

    public ArrayView<T> this[System.Range range]
        => buffer.get_view(range);

    public void resize(int new_capacity) {
        if (new_capacity < 0)
            throw new ArgumentOutOfRangeException(nameof(new_capacity));

        if(new_capacity < length)
            throw new ArgumentOutOfRangeException(nameof(new_capacity));

        if (new_capacity == buffer.length)
            return;

        Console.WriteLine($"Buffer.resize: {capacity} -> {new_capacity}");

        buffer.resize(capacity);
    }

    public void insert(int index, T[] items) {
        buffer.insert(items, index);
    }

    public void insert(int index, T item) {
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (index + 1 > buffer.length)
            throw new ArgumentOutOfRangeException(nameof(index));

        buffer[index] = item;
    }

    public int append(T[] items) {
        var position = length;
        if(capacity < length + items.Length)
           resize(length + items.Length);
        length += items.Length;
        insert(position, items);
        return position;
    }

    public int append(T item) {
        var position = length;
        if(capacity < length + 1)
            resize(length + 1);
        length += 1;
        insert(position, item);
        return position;
    }

    public sealed override int item_size => Unsafe.SizeOf<T>();

    protected void bind_buffer() {
        // Console.WriteLine("Binding " + ToString());

        if (handle == 0)
            throw new NotSupportedException("no handle has been allocated yet!");

        GL.BindBuffer(target, handle);
    }

    public sealed override void create() => create(BufferUsageHint.StaticDraw);

    public virtual void create(BufferUsageHint usage) {
        Debug.assert(handle == 0);
        if (handle == 0)
            handle = GL.GenBuffer();

        bind_buffer();
        GL.BufferData(target, length * item_size, buffer.get_address(), usage);

        status = Status.Uploaded;
        version = Engine.frame;
    }

    public override void update() {
        if (handle == 0)
            Error.not_allocated(this);

        bind_buffer();
        GL.BufferSubData(target, IntPtr.Zero, length * item_size, buffer.get_address());

        status = Status.Modified;
        version = Engine.frame;
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