namespace NetGL;

public struct RenderSettings {
    public bool depth_test;
    public bool cull_face;
    public bool blending;
    public bool wireframe;
    public bool front_facing;
    public bool scissor_test;

    public RenderSettings() {
        this.depth_test   = RenderState.depth_test.value;
        this.cull_face    = RenderState.cull_face.value;
        this.blending     = RenderState.blending.value;
        this.wireframe    = RenderState.wireframe.value;
        this.front_facing = RenderState.front_facing.value;
        this.scissor_test = RenderState.scissor_test.value;
    }

    public RenderSettings(bool depth_test, bool cull_face, bool blending, bool wireframe, bool front_facing, bool scissor_test) {
        this.depth_test   = depth_test;
        this.cull_face    = cull_face;
        this.blending     = blending;
        this.wireframe    = wireframe;
        this.front_facing = front_facing;
        this.scissor_test = scissor_test;
    }

    public uint hash => (uint) (
        (depth_test ? 1 : 0) |
        (cull_face ? 2 : 0) |
        (blending ? 4 : 0) |
        (wireframe ? 8 : 0) |
        (front_facing ? 16 : 0) |
        (scissor_test ? 32 : 0)
    );

    public static bool operator ==(RenderSettings left, RenderSettings right) => left.hash == right.hash;
    public static bool operator!=(RenderSettings left, RenderSettings right) => left.hash != right.hash;
}