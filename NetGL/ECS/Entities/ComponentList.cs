using System.Collections;

namespace NetGL.ECS;

public class ReadOnlyComponentList: IReadOnlyCollection<IComponent> {
    protected readonly List<IComponent> list = [];

    public int count => list.Count;

    public enum Compare {
        And,
        Or
    }

    public IEnumerable<IComponent> this[Type type1, Compare compare, Type type2] {
        get {
            foreach(var component in list)
                if (component.GetType() == type1 && component.GetType() == type2)
                    yield return component;
        }
    }

    public IComponent this[Type type] {
        get {
            foreach(var component in list)
                if (component.GetType() == type)
                    return component;
            throw new IndexOutOfRangeException($"{nameof(type)}:{type}");
        }
    }

    public IComponent this[int index] => list[index];

    public IComponent this[string name]{
        get {
            foreach(var component in list)
                if (component.name == name)
                    return component;
            throw new IndexOutOfRangeException($"{nameof(name)}:{name}");
        }
    }

    public ReadOnlyComponentList with_name(in string name) {
        var result = new ComponentList();

        foreach(var component in list)
            if (component.name == name)
                result.add(component);

        return result;
    }

    public IEnumerator<IComponent> GetEnumerator() => list.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => list.GetEnumerator();
    int IReadOnlyCollection<IComponent>.Count => list.Count;
}

public class ComponentList: ReadOnlyComponentList, ICollection<IComponent> {
    public void add(in IComponent entity) => list.Add(entity);

    void ICollection<IComponent>.Add(IComponent component) => list.Add(component);
    void ICollection<IComponent>.Clear() => list.Clear();
    bool ICollection<IComponent>.Contains(IComponent component) => list.Contains(component);
    void ICollection<IComponent>.CopyTo(IComponent[] array, int arrayIndex) => list.CopyTo(array, arrayIndex);
    bool ICollection<IComponent>.Remove(IComponent component) => list.Remove(component);
    bool ICollection<IComponent>.IsReadOnly => false;
    int ICollection<IComponent>.Count => list.Count;
}