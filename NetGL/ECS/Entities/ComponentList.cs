using System.Collections;

namespace NetGL.ECS;

public class ReadOnlyComponentList {
    protected readonly List<IComponent> list = [];

    public int count => list.Count;

    public C get<C>() {
        for (int i = 0; i < list.Count; ++i)
            if (list[i] is C component)
                return component;

        throw new IndexOutOfRangeException(nameof(C));
    }

    public IEnumerable<C> get_all<C>() {
        for (var i = 0; i < list.Count; ++i)
            if (list[i] is C component)
                yield return component;
    }

    public void for_each<C>(Action<C> action) {
        for (int i = 0; i < list.Count; ++i)
            if (list[i] is C component)
                action(component);
    }

    public struct Enumerator: IEnumerator<IComponent> {
        private readonly List<IComponent> _list;
        private int _index;

        public Enumerator(in List<IComponent> list) {
            _list  = list;
            _index = -1;
        }

        public IComponent Current => _list[_index];
        object IEnumerator.Current => Current;
        public bool MoveNext() => ++_index < _list.Count;
        public void Reset() => _index = -1;

        public void Dispose() { }
    }

    public Enumerator GetEnumerator() => new Enumerator(list);
}

public class ComponentList: ReadOnlyComponentList {
    internal ComponentList() {}

    internal ComponentList(List<IComponent> components) {
        foreach (var c in components)
            add(c);
    }

    public void add(in IComponent component) {
        list.Add(component);
    }
}