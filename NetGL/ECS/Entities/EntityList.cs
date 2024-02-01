using System.Collections;

namespace NetGL.ECS;

public class ReadOnlyEntityList: IReadOnlyCollection<Entity> {
    protected readonly List<Entity> list = [];

    public int count => list.Count;

    public ReadOnlyEntityList recursive {
        get {
            var result = new EntityList();
            foreach (var entity in list) {
                result.add(entity);
                foreach (var child in entity.children.recursive) {
                    result.add(child);
                }
            }

            return result;
        }
    }

    public ReadOnlyEntityList with_name(in string name) {
        var result = new EntityList();

        foreach(var entity in list)
            if (entity.name == name)
                result.add(entity);

        return result;
    }

    public ReadOnlyEntityList with_component<C1>() where C1: class, IComponent {
        var result = new EntityList();

        foreach(var entity in list)
            if (entity.has<C1>())
                result.add(entity);

        return result;
    }

    public ReadOnlyEntityList with_component<C1, C2>() where C1: class, IComponent where C2: class, IComponent {
        var result = new EntityList();

        foreach(var entity in list)
            if (entity.has<C1, C2>())
                result.add(entity);

        return result;
    }


    public IEnumerator<Entity> GetEnumerator() => list.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => list.GetEnumerator();
    int IReadOnlyCollection<Entity>.Count => list.Count;
}

public class EntityList: ReadOnlyEntityList, ICollection<Entity> {
    public void add(Entity entity) => list.Add(entity);

    void ICollection<Entity>.Add(Entity entity) => list.Add(entity);
    void ICollection<Entity>.Clear() => list.Clear();
    bool ICollection<Entity>.Contains(Entity entity) => list.Contains(entity);
    void ICollection<Entity>.CopyTo(Entity[] array, int arrayIndex) => list.CopyTo(array, arrayIndex);
    bool ICollection<Entity>.Remove(Entity entity) => list.Remove(entity);
    bool ICollection<Entity>.IsReadOnly => false;
    int ICollection<Entity>.Count => list.Count;
}