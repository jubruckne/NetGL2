using System.Numerics;
using NetGL.ECS;

namespace NetGL;

using BulletSharp;
using System.Collections.Generic;

public class Physics {
    ///create 125 (5x5x5) dynamic objects
    private const int ArraySizeX = 5, ArraySizeY = 5, ArraySizeZ = 5;

    private Vector3 _startPosition = new Vector3(-5, 1, -3) - new Vector3(ArraySizeX / 2f, 0, ArraySizeZ / 2f);

    public DiscreteDynamicsWorld World { get; }

    private CollisionDispatcher _dispatcher;
    private DbvtBroadphase _broadphase;
    private List<CollisionShape> _collisionShapes = new List<CollisionShape>();
    private CollisionConfiguration _collisionConf;

    public Physics() {
        // collision configuration contains default setup for memory, collision setup
        _collisionConf = new DefaultCollisionConfiguration();
        _dispatcher = new CollisionDispatcher(_collisionConf);

        _broadphase = new DbvtBroadphase();
        World = new DiscreteDynamicsWorld(_dispatcher, _broadphase, null, _collisionConf);


        // create a few dynamic rigidbodies
        var colShape = new BoxShape(1);
        _collisionShapes.Add(colShape);

        float mass = 1.0f;
        Vector3 localInertia = colShape.CalculateLocalInertia(mass);

        var rbInfo = new RigidBodyConstructionInfo(mass, null, colShape, localInertia);

        for (int y = 0; y < ArraySizeY; y++) {
            for (int x = 0; x < ArraySizeX; x++) {
                for (int z = 0; z < ArraySizeZ; z++) {
                    Matrix4x4 startTransform = Matrix4x4.CreateTranslation(
                        _startPosition + 2 * new Vector3(x, y, z));

                    // using motionstate is recommended, it provides interpolation capabilities
                    // and only synchronizes 'active' objects
                    rbInfo.MotionState = new DefaultMotionState(startTransform);
                    var body = new RigidBody(rbInfo);

                    // make it drop from a height
                    body.Translate(new Vector3(0, 15, 0));

                    World.AddRigidBody(body);
                }
            }
        }

        rbInfo.Dispose();
    }

    public virtual void Update(float elapsedTime) {
        World.StepSimulation(elapsedTime);
    }

    public void ExitPhysics() {
        // remove/dispose constraints
        for (int i = World.NumConstraints - 1; i >= 0; i--) {
            TypedConstraint constraint = World.GetConstraint(i);
            World.RemoveConstraint(constraint);
            constraint.Dispose();
        }

        // remove the rigidbodies from the dynamics world and delete them
        for (int i = World.NumCollisionObjects - 1; i >= 0; i--) {
            CollisionObject obj = World.CollisionObjectArray[i];
            RigidBody body = obj as RigidBody;
            if (body != null && body.MotionState != null) {
                body.MotionState.Dispose();
            }

            World.RemoveCollisionObject(obj);
            obj.Dispose();
        }

        // delete collision shapes
        foreach (CollisionShape shape in _collisionShapes) {
            shape.Dispose();
        }

        _collisionShapes.Clear();

        World.Dispose();
        _broadphase.Dispose();
        if (_dispatcher != null) {
            _dispatcher.Dispose();
        }

        _collisionConf.Dispose();
    }

    private RigidBody CreateDynamicBody(float mass, Matrix4x4 startTransform, CollisionShape shape) {
        Vector3 localInertia = shape.CalculateLocalInertia(mass);
        return CreateBody(mass, startTransform, shape, localInertia);
    }

    private RigidBody CreateBody(float mass, Matrix4x4 startTransform, CollisionShape shape, Vector3 localInertia) {
        var motionState = new DefaultMotionState(startTransform);
        using (var rbInfo = new RigidBodyConstructionInfo(mass, motionState, shape, localInertia)) {
            var body = new RigidBody(rbInfo);
            World.AddRigidBody(body);
            return body;
        }
    }
}

public static class PhysicsExt {
    public static Component<RigidBody> add_rigid_body(this Entity entity, in RigidBody body) {
        return add_rigid_body(entity, body.GetType().Name, body);
    }

    public static Component<RigidBody> add_rigid_body(this Entity entity, string name, in RigidBody body) {
        entity.world.physics.World.AddRigidBody(body);
        entity.add(name, body);
        return entity.get<Component<RigidBody>>();
    }

    public static Component<RigidBody> add_rigid_body(this Entity entity, string? name = null, float mass = 1f) {
        var body = new RigidBody(
            new RigidBodyConstructionInfo(
                mass,
                new DefaultMotionState(),
                new BoxShape(.001f)
            )
        );
        return add_rigid_body(entity, name ?? body.GetType().Name, body);
    }
}