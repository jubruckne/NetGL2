using NetGL.Vectors;
using OpenTK.Mathematics;

namespace NetGL.Materials;

public interface IShader;

public interface IShader<TVertexBuffer>: IShader
    where TVertexBuffer: IVertexBuffer;

public interface IShader<TVertexBuffer, TUniformBuffer>: IShader<TVertexBuffer>
    where TVertexBuffer: IVertexBuffer
    where TUniformBuffer: IUniformBuffer {
}

public interface IMaterial;

public interface IMaterial<TShader>: IMaterial
    where TShader: IShader;


public class Shader<TVertexBuffer, TUniformBuffer>: IShader<TVertexBuffer, TUniformBuffer>
    where TVertexBuffer: IVertexBuffer
    where TUniformBuffer: IUniformBuffer {
}

public class DefaultShader: IShader<VertexBuffer<float3, float3>, UniformBuffer<Matrix4>> {
}

public class DefaultMaterial: IMaterial<DefaultShader> {
    public DefaultShader shader { get; }

    public DefaultMaterial(DefaultShader shader) {
        this.shader = shader;
    }
}