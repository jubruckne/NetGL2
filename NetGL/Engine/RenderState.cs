using OpenTK.Graphics.OpenGL4;

namespace NetGL;

public class RenderState {
    public Shader shader;
    public bool depth_test;
    public bool cull_face;
    public bool blending;
    public bool wireframe;
    public bool front_facing;

    public void bind() {
        shader.bind();

        if(depth_test)
            GL.Enable(EnableCap.DepthTest);
        else
            GL.Disable(EnableCap.DepthTest);

        if(cull_face)
            GL.Enable(EnableCap.CullFace);
        else
            GL.Disable(EnableCap.CullFace);

        if (blending) {
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        } else {
            GL.Disable(EnableCap.Blend);
        }

        GL.PolygonMode(MaterialFace.FrontAndBack, wireframe ? PolygonMode.Line : PolygonMode.Fill);
        GL.FrontFace(front_facing ? FrontFaceDirection.Ccw : FrontFaceDirection.Cw);
    }
}