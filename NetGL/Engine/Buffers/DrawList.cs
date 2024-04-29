using OpenTK.Graphics.OpenGL4;

namespace NetGL;

public interface IDrawCommand {
    void execute();
};

public static class DrawCommand {
    public class List: Bag<IDrawCommand>, IDrawCommand {
        public void execute() {
            foreach (var command in this)
                command.execute();
        }
    }

    public readonly struct Bind: IDrawCommand {
        public readonly IBindable bindable;

        public Bind(IBindable bindable)
            => this.bindable = bindable;

        public void execute()
            => bindable.bind();
    }

    public readonly struct SetState: IDrawCommand {
        public readonly IState state;

        public SetState(IState state)
            => this.state = state;

        public void execute()
            => RenderState.bind(state);
    }

    public readonly struct DrawArrays: IDrawCommand {
        public readonly IVertexBuffer vertex_buffer;
        public readonly PrimitiveType primitive_type;
        public readonly int? vertex_count = null;

        public DrawArrays(IVertexBuffer vertex_buffer, PrimitiveType primitive_type, int? vertex_count = null) {
            this.vertex_buffer = vertex_buffer;
            this.primitive_type = primitive_type;
            this.vertex_count = vertex_count;
        }

        public void execute() {
            GL.DrawArrays(primitive_type, 0, vertex_count ?? vertex_buffer.length);
        }
    }

    public readonly struct MultiDrawElementsBaseVertex: IDrawCommand {
        public readonly IIndexBuffer index_buffer;
        public readonly VertexArrayIndexed.DrawRanges draw_ranges;

        public MultiDrawElementsBaseVertex(IIndexBuffer index_buffer, VertexArrayIndexed.DrawRanges draw_ranges) {
            this.index_buffer = index_buffer;
            this.draw_ranges  = draw_ranges;
        }

        public void execute() {
            GL.MultiDrawElementsBaseVertex(
                                           index_buffer.primitive_type,
                                           draw_ranges.counts,
                                           index_buffer.draw_element_type,
                                           draw_ranges.indices,
                                           draw_ranges.length,
                                           draw_ranges.base_vertices
                                          );
        }
    }

    public readonly struct DrawElements: IDrawCommand {
        public readonly IIndexBuffer index_buffer;
        public readonly int? index_count = null;

        public DrawElements(IIndexBuffer index_buffer, int? index_count = null) {
            this.index_buffer = index_buffer;
            this.index_count = index_count;
        }

        public void execute() {
            GL.DrawElements(
                            index_buffer.primitive_type,
                            (index_count ?? index_buffer.length) * 3,
                            index_buffer.draw_element_type,
                            0);
        }
    }

    public readonly struct DrawElementsInstanced: IDrawCommand {
        public readonly IIndexBuffer index_buffer;
        public readonly IVertexBuffer instance_buffer;
        public readonly int? index_count = null;
        public readonly int? instance_count = null;

        public DrawElementsInstanced(IIndexBuffer index_buffer, IVertexBuffer instance_buffer, int? index_count = null, int? instance_count = null) {
            this.index_buffer = index_buffer;
            this.instance_buffer = instance_buffer;
            this.index_count = index_count;
            this.instance_count = instance_count;
        }

        public void execute() {
            GL.DrawElementsInstanced(
                                     index_buffer.primitive_type,
                                     (index_count ?? index_buffer.length) * 3,
                                     index_buffer.draw_element_type,
                                     IntPtr.Zero,
                                     instance_count ?? instance_buffer.length
                                     );
        }
    }
}