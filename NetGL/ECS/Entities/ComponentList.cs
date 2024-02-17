using System.Collections;

namespace NetGL.ECS;

public class ReadOnlyComponentList: IEnumerable<IComponent> {
    protected readonly Dictionary<Type, List<IComponent>> dict = [];
    protected readonly List<IComponent> list = [];

    public int count => list.Count;
    public IComponent this[int index] => list[index];

    public T get<T>() where T: IComponent {
        if (dict.TryGetValue(typeof(T), out var this_list))
            return (T)this_list[0];
        throw new IndexOutOfRangeException(typeof(T).Name);
    }

    public ReadOnlyComponentList with_name(in string name) {
        var result = new ComponentList();

        foreach(var component in list)
            if (component.name == name)
                result.add(component);

        return result;
    }

    public ReadOnlyComponentList of_type<T>() where T: IComponent {
        if (dict.TryGetValue(typeof(T), out var this_list))
            return new ComponentList(this_list);

        return new ComponentList();
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

        var type = component.GetType();

        if (dict.TryGetValue(type, out var this_list)) {
            this_list.Add(component);
        } else {
            this_list = new List<IComponent>();
            this_list.Add(component);
            dict.Add(type, this_list);
        }

        list.Add(component);
    }
}