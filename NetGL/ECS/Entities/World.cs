namespace NetGL.ECS;

using System.Numerics;
using BulletSharp;
using OpenTK.Mathematics;
using Vector3 = OpenTK.Mathematics.Vector3;

public class World: Entity {
    private readonly EntityList world_entities;
    internal readonly Physics physics;

    public World(): base("World", null, null) {
        world_entities = new();
        physics = new();
        add(new Physics());

        // create the ground
        var groundShape = new BoxShape(10, 50, 10);
        var v = this.add_rigid_body("Ground", new RigidBody(new RigidBodyConstructionInfo(0, new DefaultMotionState(), groundShape)));
        v.data.Translate(new System.Numerics.Vector3(0f, -53f, 0f));
    }

    public void for_all_components_with<C1>(Action<C1> action) where C1: class, IComponent {
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

    public Entity create_entity(string name, Entity? parent = null, Transform? transform = null) {
        var e = new Entity(name, parent ?? this, transform);
        world_entities.add(e);

        return e;
    }

    internal void add_entity<T>(T e) where T: Entity {
        world_entities.add(e);
    }

    public void render() {
        foreach (var entity in children.with_component<Camera>()) {
            foreach(var cam in entity.get_all<Camera>()) {
                // Console.WriteLine($"Switching to cam {cam.name}");
                //render_items.Sort(new Comparison<RenderItem>((item, renderItem) => ));

                if (cam.entity.parent != null) {
                    // Console.WriteLine($"Switching to viewport {cam.viewport}");
                    cam.viewport.make_current();
                    cam.viewport.clear();
                    Debug.assert_opengl();

                    foreach (var child in cam.entity.parent.children) {
                        // Console.WriteLine("  " + child.name);
                        render_entity(
                            child,
                            cam.projection_matrix,
                            cam.camera_matrix,
                            cam.transform.position,
                            Matrix4.Identity);
                    }
                } else {
                    Console.WriteLine($"Empty camera: {cam}");
                }
            }

            Debug.assert_opengl();
        }
    }

    private void render_entity(in Entity entity, in Matrix4 projection_matrix, in Matrix4 camera_matrix, in Vector3 camera_pos, in Matrix4 parent_model_matrix) {
        var model_matrix = entity.transform.calculate_model_matrix() * parent_model_matrix;

        /*entity.for_any_component_like<AmbientLight, DirectionalLight, PointLight>(
            component => lights.Add((ILight)component)
        );*/

        foreach (var renderable in entity.components.get_all<IRenderableComponent>())
            renderable.render(projection_matrix, camera_matrix, camera_pos, model_matrix);

        foreach (var child in entity.children)
            render_entity(child, projection_matrix, camera_matrix, camera_pos, model_matrix);
    }

    public void update(float game_time, float delta_time) {
        /*if (parallel) {
            Parallel.ForEach(children, entity => update_entity_pre_physics(entity));

            physics.World.StepSimulation(delta_time);

            Parallel.ForEach(children, entity => update_entity(game_time, delta_time, entity));
        } else {
        */
            foreach (var entity in children) {
                update_entity_pre_physics(entity);
            }

            physics.World.StepSimulation(delta_time);

            foreach (var entity in children) {
                update_entity(game_time, delta_time, entity);
            }
      //  }
    }

    private void update_entity_pre_physics(in Entity entity) {
        if (entity.try_get<RigidBody>(out var body)) {
             body.CenterOfMassTransform = Matrix4x4.CreateTranslation(entity.transform.position.X,
                entity.transform.position.Y, entity.transform.position.Z);
        }

        foreach (Entity child in entity.children) {
            update_entity_pre_physics(child);
        }
    }

    private void update_entity(in float game_time, in float delta_time, in Entity entity) {
        if (entity.try_get<RigidBody>(out var body)) {
            entity.transform.position.X = body.CenterOfMassPosition.X;
            entity.transform.position.Y = body.CenterOfMassPosition.Y;
            entity.transform.position.Z = body.CenterOfMassPosition.Z;
        }

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