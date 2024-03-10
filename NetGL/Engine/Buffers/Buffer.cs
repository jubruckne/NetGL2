using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;

namespace NetGL;

public interface IBuffer {
    int count { get; }
    int item_size{ get; }
    Type item_type { get; }
    int size { get; }
    void upload();
    void bind();
    void unbind();
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
    public abstract void unbind();

    public Status status {
        get;
        protected set;
    }
}

public abstract class Buffer<T>: Buffer where T: struct {
    protected T[] buffer;
    private readonly BufferTarget target;
    private int handle;
    
    protected Buffer(): this(BufferTarget.ArrayBuffer, 0) { }
    
    protected Buffer(BufferTarget target, int count = 0) {
        this.target = target;
        this.handle = 0;
        buffer = new T[count];
    }

    public override Type item_type => typeof(T);
    
    public ref T this[int index] {
        get {
            if (index < 0 || index >= buffer.Length)
                throw new IndexOutOfRangeException($"Index out of range: {index}!");

            return ref buffer[index];
        }
    }
    
    public void resize(int new_size) {
        if (new_size < 0)
            throw new ArgumentOutOfRangeException(nameof(new_size));

        if (new_size == buffer.Length)
            return;

        Console.WriteLine($"Buffer.resize: {buffer.Length} -> {new_size}");
        
        var resized = new T[new_size];
        Array.Copy(buffer, resized, Math.Min(buffer.Length, new_size));

        buffer = resized;
    }

    public void insert(int index, in T[] items) {
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (index + items.Length > buffer.Length)
            throw new ArgumentOutOfRangeException(nameof(index));

        items.CopyTo(buffer, index);
    }

    public void insert(int index, in T item) {
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (index + 1 > buffer.Length)
            throw new ArgumentOutOfRangeException(nameof(index));

        buffer[index] = item;
    }


    public int append(in T[] items) {
        var position = buffer.Length;
        resize(buffer.Length + items.Length);
        insert(position, items);
        return position;
    }

    public int append(in T item) {
        int position = buffer.Length;
        resize(buffer.Length + 1);
        insert(position, item);
        return position;
    }

    
    public override int count { 
        get => buffer.Length;
    }

    public override int item_size { get => Marshal.SizeOf(new T()); }
    public override int size { get => item_size * count; }

    public override void bind() {
        // Console.WriteLine("Buffer.bind()");

        if (handle == 0)
            throw new NotSupportedException("no handle has been allocated yet!");

        GL.BindBuffer(target, handle);
    }

    public override void unbind() {
        // Console.WriteLine("Buffer.unbind()");
        
        if (handle == 0)
            throw new NotSupportedException("no handle has been allocated yet!");

        GL.BindBuffer(target, 0);
    }

    public override void upload() {
        if (handle == 0) {
            handle = GL.GenBuffer();
        }

        GL.BindBuffer(target, handle);
        GL.BufferData(target, buffer.Length * item_size, buffer, BufferUsageHint.StaticDraw);
        GL.BindBuffer(target, 0);

        status = Status.Uploaded;
    }
}