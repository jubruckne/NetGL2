using OpenTK.Mathematics;

namespace NetGL.ECS;

public class World: Entity {
    private readonly List<Entity> world_entities;

    public World(): base("World", null, null) {
        world_entities = new List<Entity>();
    }

    public void for_all_components_with<C1>(Action<C1> action) where C1: IComponent {
        foreach(var entity in world_entities)
            if (entity.has<C1>())
                action(entity.get<C1>());
    }

    public Entity get(string name) {
        foreach (var ent in world_entities) {
            if (ent.name == name) return ent;
        }

        throw new IndexOutOfRangeException(nameof(name));
    }

    public Entity create_entity(string name, Entity? parent = null, Transform? tranform = null) {
        var e = new Entity(name, parent ?? this, tranform);
        world_entities.Add(e);

        return e;
    }

    private Entity get_camera_entity() {
        foreach (var entity in children) {
            if (entity.has<FirstPersonCamera>()) {
                return entity;
            }
        }

        throw new InvalidOperationException("no camera found!");
    }

    public void render() {
        var cam = get_camera_entity().get<FirstPersonCamera>();

        foreach (var entity in children) {
            render_entity(
                entity,
                cam.projection_matrix,
                cam.camera_matrix,
                Matrix4.Identity);
        }

        Error.check();
    }

    private void render_entity(in Entity entity, in Matrix4 projection_matrix, in Matrix4 camera_matrix, in Matrix4 parent_model_matrix) {
        var model_matrix = Matrix4.LookAt(entity.transform.position, entity.transform.attitude.direction + entity.transform.position, entity.transform.attitude.up).Inverted() * parent_model_matrix;

        /*entity.for_any_component_like<AmbientLight, DirectionalLight, PointLight>(
            component => lights.Add((ILight)component)
        );*/

        foreach (var renderable in entity.get_renderable_components())
            renderable.render(projection_matrix, camera_matrix, model_matrix);

        foreach (var child in entity.children)
            render_entity(child, projection_matrix, camera_matrix, model_matrix);
    }

    public void update(in float game_time, in float delta_time) {
        foreach (var entity in children) {
            update_entity(game_time, delta_time, entity);
        }
    }

    private void update_entity(in float game_time, in float delta_time, in Entity entity) {
        foreach (var updateable in entity.get_updateable_components()) {
            //Console.WriteLine("update: " + updateable);
            if(updateable.enable_update)
                updateable.update(game_time, delta_time);
        }

        foreach (Entity child in entity.children) {
            update_entity(game_time, delta_time, child);
        }
    }
}