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

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public List<IComponent>.Enumerator GetEnumerator() => list.GetEnumerator();
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