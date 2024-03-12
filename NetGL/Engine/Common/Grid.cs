using System.Collections;
using System.Diagnostics.Tracing;

namespace NetGL;

public class Grid<T>: IEnumerable<T> where T: class {
    public delegate T AllocationDelegate(short x, short y);

    private readonly Dictionary<int, T> data;
    private readonly AllocationDelegate on_allocate;

    public Grid(AllocationDelegate on_allocate) {
        this.data = [];
        this.on_allocate = on_allocate;
    }

    public T allocate(short x, short y) => this[x, y];

    public bool is_allocated(short x, short y) => data.ContainsKey(y << 16 | (x & 0xFFFF));

    public T this[short x, short y] {
        get {
            if (data.TryGetValue(y << 16 | (x & 0xFFFF), out var value)) return value;
            value = on_allocate(y, x);
            data[y << 16 | (x & 0xFFFF)] = value;

            return value;
        }
    }

    public void clear() => data.Clear();
    public IEnumerator<T> GetEnumerator() => data.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}