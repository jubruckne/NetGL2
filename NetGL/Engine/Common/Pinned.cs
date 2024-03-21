namespace NetGL;

using System.Runtime.InteropServices;

public class Pinned<T>: IDisposable where T: class {
    public Pinned(T obj) => handle = GCHandle.Alloc(obj, GCHandleType.Pinned);
    public T? obj => handle?.Target as T;

    private GCHandle? handle;

    private void release() {
        handle?.Free();
        handle = null;
    }

    void IDisposable.Dispose() {
        GC.SuppressFinalize(this);
    }

    ~Pinned() => release();
}