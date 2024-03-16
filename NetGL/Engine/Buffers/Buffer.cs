using System.Runtime.CompilerServices;
using OpenTK.Mathematics;

namespace NetGL;

using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;

public interface IBindable {
    void bind();
}

public interface IBuffer: IBindable {
    int count { get; }
    int item_size{ get; }
    Type item_type { get; }
    int size { get; }
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

    public abstract int count { get; }
    public abstract int item_size { get; }
    public abstract Type item_type { get; }
    public abstract int size { get; }

    public abstract void upload();
    public abstract void bind();

    public Status status {
        get;
        protected set;
    }
}

public abstract class Buffer<T>: Buffer, IDisposable where T: unmanaged {
    protected readonly NativeArray<T> buffer;
    private readonly BufferTarget target;
    private int handle;

    ~Buffer() {
        buffer.Dispose();
    }

    protected Buffer(BufferTarget target, in T[] items) {
        this.target = target;
        this.handle = 0;
        buffer = new NativeArray<T>(items);
    }

    /*
    protected Buffer(BufferTarget target, in Vector3i[] items) {
        this.target = target;
        this.handle = 0;
        buffer = new NativeArray<T>(Unsafe.SizeOf<Vector3i>() * items.Length);
        buffer.copy_from(items);
    }*/

    protected Buffer(BufferTarget target, in ReadOnlySpan<T> items) {
        this.target = target;
        this.handle = 0;
        buffer = new NativeArray<T>(items);
    }

    protected Buffer(BufferTarget target, int count = 0) {
        this.target = target;
        this.handle = 0;
        buffer = new NativeArray<T>(count);
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

    public override int count { 
        get => buffer.length;
    }

    public override int item_size { get => Marshal.SizeOf(new T()); }
    public override int size { get => item_size * count; }

    public override void bind() {
        Console.WriteLine("Buffer.bind: " + GetType().GetFormattedName());

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
}