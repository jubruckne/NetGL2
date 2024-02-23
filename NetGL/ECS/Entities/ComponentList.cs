using System.Collections;

namespace NetGL.ECS;

public class ReadOnlyComponentList: IEnumerable<IComponent> {
    protected readonly List<IComponent> list = [];

    public int count => list.Count;

    public C get<C>() {
        for(int i = 0; i < list.Count; ++i)
            if (list[i] is C component) return component;

        throw new IndexOutOfRangeException(nameof(C));
    }

    public IEnumerable<C> get_all<C>() {
        for(int i = 0; i < list.Count; ++i)
            if (list[i] is C component) yield return component;
    }

    public void for_each<C>(Action<C> action) {
        for(int i = 0; i < list.Count; ++i)
            if (list[i] is C component) action(component);
    }

    IEnumerator<IComponent> IEnumerable<IComponent>.GetEnumerator() => list.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => list.GetEnumerator();
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