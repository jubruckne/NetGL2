using OpenTK.Mathematics;

namespace NetGL.ECS;

public class World {
    private readonly List<Entity> world_entities;

    public World() {
        world_entities = new List<Entity>();
    }

    public IEnumerable<Entity> root{
        get {
            foreach (Entity ent in world_entities) {
                if (ent.parent == null) yield return ent;
            }
        }
    }

    public Entity get(string name) {
        foreach (var ent in world_entities) {
            if (ent.name == name) return ent;
        }

        throw new IndexOutOfRangeException(nameof(name));
    }

    public Entity create_entity(string name, Entity? parent = null, Transform? tranform = null) {
        var e = new Entity(name, parent, tranform);
        world_entities.Add(e);

        return e;
    }

    private Entity get_camera_entity() {
        foreach (var entity in root) {
            if (entity.has<FirstPersonCamera>()) {
                return entity;
            }
        }

        throw new InvalidOperationException("no camera found!");
    }

    public void render() {
        var cam = get_camera_entity().get<FirstPersonCamera>();

        foreach (var entity in root) {
            render_entity(entity, cam.projection_matrix, cam.camera_matrix, Matrix4.Identity);
        }

        Error.check();
    }

    private void render_entity(in Entity entity, in Matrix4 projection_matrix, in Matrix4 camera_matrix, in Matrix4 parent_model_matrix) {
        var model_matrix = Matrix4.LookAt(entity.transform.position, entity.transform.forward + entity.transform.position, entity.transform.up).Inverted() * parent_model_matrix;

        foreach (var renderable in entity.get_renderable_components())
            renderable.render(projection_matrix, camera_matrix, model_matrix);

        foreach (Entity child in entity.children)
            render_entity(child, projection_matrix, camera_matrix, model_matrix);
    }

    public void update(in float delta_time) {
        foreach (var entity in root) {
            update_entity(delta_time, entity);
        }
    }

    private void update_entity(in float delta_time, in Entity entity) {
        foreach (var updateable in entity.get_updateable_components()) {
            Console.WriteLine("update: " + updateable);
            updateable.update(delta_time);
        }

        foreach (Entity child in entity.children) {
            update_entity(delta_time, child);
        }
    }

    /*
    public void update_systems(in float delta_time) {
        foreach (var sys in systems) {
            sys.system.update(sys.entities, delta_time);
        }
    }

    public void render_systems() {
        foreach (var sys in systems) {
            sys.system.render(sys.entities);
        }
    }
    */
}