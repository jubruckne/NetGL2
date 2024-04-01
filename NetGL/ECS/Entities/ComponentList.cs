using System.Collections;
using System.Runtime.CompilerServices;

namespace NetGL.ECS;

public class ReadOnlyComponentList {
    protected readonly List<IComponent> list = [];

    public int count => list.Count;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public C get<C>() {
        for (int i = 0; i < list.Count; ++i)
            if (list[i] is C component)
                return component;

        throw new IndexOutOfRangeException(nameof(C));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public IEnumerable<C> get_all<C>() {
        for (var i = 0; i < list.Count; ++i)
            if (list[i] is C component)
                yield return component;
    }

    public struct Enumerator: IEnumerator<IComponent> {
        private readonly List<IComponent> _list;
        private int _index;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator(in List<IComponent> list) {
            _list  = list;
            _index = -1;
        }

        public IComponent Current {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _list[_index];
        }

        object IEnumerator.Current => Current;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext() => ++_index < _list.Count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset() => _index = -1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() { }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public Enumerator GetEnumerator() => new Enumerator(list);
}

public sealed class ComponentList: ReadOnlyComponentList {
    internal ComponentList() {}

    internal ComponentList(List<IComponent> components) {
        foreach (var c in components)
            add(c);
    }

    public void add(in IComponent component) {
        list.Add(component);
    }
}