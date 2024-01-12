using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace NetGL.ECS;

public static class SpherePrefab {
    public static Entity create_sphere(this World world, string name, Entity? parent = null, float radius = 1f) {
        var entity = world.create_entity(name);

        if(parent != null)
            world.entity_add_component<ParentComponent>(entity, new ParentComponent(parent));

        var sphere_geometry = Sphere.create_geometry(radius, 32, 24);

        var va = new VertexArray(PrimitiveType.Triangles);
        ArrayBuffer<Vertex> vb = new(sphere_geometry.vertices);
        IndexBuffer ib = new(sphere_geometry.triangles);
        vb.upload();
        ib.upload();
        va.upload(vb, ib);

        return entity;
    }
}