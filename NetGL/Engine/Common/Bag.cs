using System.Collections;
using System.Runtime.InteropServices;
using NetGL.ECS;

namespace NetGL;

using System.Text;

public class Bag<TItem> where TItem: class {
    public delegate bool ItemPredicate(ref TItem item);
    public delegate bool ItemPredicate<in T>(ref TItem item, T arg1);
    public delegate bool ItemPredicate<in T1, in T2>(ref TItem item, T1 arg1, T2 arg);

    public delegate void ItemDelegate(ref TItem item);
    public delegate void IndexItemDelegate(int index, ref TItem item);

    public event ItemDelegate? on_added;
    public event ItemDelegate? on_removed;

    protected readonly List<TItem> list;

    public Bag(): this(4) {}
    public Bag(int capacity) => list = new(capacity);

    public int length => list.Count;

    public virtual void add(in TItem item) {
        list.Add(item);
        on_added?.Invoke(ref this[^1]);
    }

    public int index_of(in TItem item) => list.IndexOf(item);

    public virtual void remove(in TItem item) {
        remove(index_of(item));
    }

    public virtual void remove(in int index) {
        ref var temp  = ref this[index];
        list.RemoveAt(index);
        on_removed?.Invoke(ref temp);
    }

    public void for_each(ItemDelegate action) {
        for (var i = 0; i < list.Count; i++) {
            action(ref this[i]);
        }
    }

    public void for_each(IndexItemDelegate action) {
        for (var i = 0; i < list.Count; i++) {
            action(i, ref this[i]);
        }
    }

    public ref TItem this[in int index] => ref CollectionsMarshal.AsSpan(list)[index];
    public ref TItem this[in Index index] => ref CollectionsMarshal.AsSpan(list)[index];
    public Span<TItem> this[in Range range] => CollectionsMarshal.AsSpan(list)[range];

    public override string ToString() {
        StringBuilder sb = new(list.Count);
        foreach (var item in list) {
            sb.Append(item);
        }
        return sb.ToString();
    }

    public Result<TItem> lookup(in ItemPredicate lookup) {
        var span = CollectionsMarshal.AsSpan(list);
        foreach (ref var item in span) {
            if (lookup(ref item)) return Result<TItem>.success(in item);
        }

        return Result<TItem>.failure();
    }

    public Result<TItem> lookup<T>(in T arg1, in ItemPredicate<T> lookup) {
        var span = CollectionsMarshal.AsSpan(list);
        foreach (ref var item in span) {
            if (lookup(ref item, arg1)) return Result<TItem>.success(in item);
        }

        return Result<TItem>.failure();
    }

    public Result<TItem> lookup<T1, T2>(in T1 arg1, in T2 arg2, in ItemPredicate<T1, T2> lookup) {
        var span = CollectionsMarshal.AsSpan(list);
        foreach (ref var item in span) {
            if (lookup(ref item, arg1, arg2)) return Result<TItem>.success(in item);
        }

        return Result<TItem>.failure();
    }

    public bool contains(in ItemPredicate lookup) {
        var span = CollectionsMarshal.AsSpan(list);
        foreach (ref var item in span) {
            if (lookup(ref item)) return true;
        }

        return false;
    }

    public struct Enumerator: IEnumerator<TItem> {
        private readonly List<TItem> _list;
        private int _index;

        public Enumerator(List<TItem> list) {
            _list  = list;
            _index = -1;
        }

        public TItem Current => _list[_index];
        object IEnumerator.Current => Current;
        public bool MoveNext() => ++_index < _list.Count;
        public void Reset() => _index = -1;
        public void Dispose() { }
    }

    public Enumerator GetEnumerator() => new Enumerator(list);
}

public class NamedBag<TItem>: Bag<string, TItem> where TItem: class, INamed {
    public NamedBag(): this(4) {}
    public NamedBag(int capacity): base(static (ref readonly TItem item) => item.name ) {}
}

public class Bag<TKey, TItem>: Bag<TItem> where TKey: IComparable<TKey> where TItem: class {
    public delegate TKey KeyDelegate(ref readonly TItem item);

    private readonly Dictionary<TKey, int> dict;
    private readonly KeyDelegate get_key;

    public Bag(in KeyDelegate get_key): this(get_key, 4) {}

    public Bag(in KeyDelegate get_key, int capacity): base(capacity) {
        this.dict    = new(capacity);
        this.get_key = get_key;
    }

    public override void add(in TItem item) {
        base.add(item);
        dict.Add(get_key(in item), length - 1);
    }

    public int index_of(in TKey key) => dict[key];

    public override void remove(in TItem item) {
        dict.Remove(get_key(in item));
        base.remove(item);
    }

    public override void remove(in int index) {
        dict.Remove(get_key(in this[index]));
        base.remove(index);
    }

    public void remove(in TKey key) => remove(dict[key]);

    public ref TItem this[in TKey key] => ref CollectionsMarshal.AsSpan(list)[dict[key]];

    public bool contains(in TKey key) => dict.ContainsKey(key);
}