using System.Numerics;
using NetGL.ECS;

namespace NetGL;

using BulletSharp;
using System.Collections.Generic;

public class Physics {
    public DiscreteDynamicsWorld World { get; }

    private readonly CollisionDispatcher _dispatcher;
    private readonly DbvtBroadphase _broadphase;
    private readonly List<CollisionShape> _collisionShapes = new List<CollisionShape>();
    private readonly CollisionConfiguration _collisionConf;

    public Physics() {
        // collision configuration contains default setup for memory, collision setup
        _collisionConf = new DefaultCollisionConfiguration();
        _dispatcher = new CollisionDispatcher(_collisionConf);

        _broadphase = new DbvtBroadphase();
        World = new DiscreteDynamicsWorld(_dispatcher, _broadphase, null, _collisionConf);
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
            RigidBody body = (RigidBody)obj;
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

    public static Component<RigidBody> add_rigid_body(this Entity entity, string? name = null, float radius = 0.5f, float mass = 1f) {
        var body = new RigidBody(
            new RigidBodyConstructionInfo(
                mass,
                new DefaultMotionState(),
                new SphereShape(radius)
            )
        );
        body.Friction = 0.25f;
        return add_rigid_body(entity, name ?? body.GetType().Name, body);
    }
}