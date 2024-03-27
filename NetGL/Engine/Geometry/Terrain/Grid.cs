using System.Collections;

namespace NetGL;

public class Grid<TData, TIndex>: IEnumerable<TData>
    where TData: class
    where TIndex: IEquatable<TIndex> {

    public delegate TData AllocationDelegate(TIndex index);

    private readonly Dictionary<TIndex, TData> data;
    private readonly AllocationDelegate on_allocate;

    public Grid(AllocationDelegate on_allocate) {
        this.data = [];
        this.on_allocate = on_allocate;
    }

    public TData allocate(TIndex index) => this[index];

    public bool is_allocated(in TIndex index) => data.ContainsKey(index);

    public TData this[TIndex index] {
        get {
            if (data.TryGetValue(index, out var value)) return value;
            value = on_allocate(index);
            data[index] = value;

            return value;
        }
    }

    public void clear() => data.Clear();
    public IEnumerator<TData> GetEnumerator() => data.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}