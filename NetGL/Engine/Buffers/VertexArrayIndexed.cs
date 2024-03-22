namespace NetGL;

using OpenTK.Graphics.OpenGL4;

public class VertexArrayIndexed: VertexArray {
    public readonly record struct DrawRange {
        public readonly int first_index;
        public readonly int last_index;
        public readonly int base_vertex;

        public int draw_count => last_index - first_index;

        public DrawRange(int first_index, int last_index, int base_vertex) {
            this.first_index = first_index;
            this.last_index = last_index;
            this.base_vertex = base_vertex;
        }

        public override string ToString()
            => $"<first={first_index}, last={last_index}, count={draw_count}, base_vertex={base_vertex}>";
    }

    public readonly IIndexBuffer index_buffer;
    public List<DrawRange>? draw_ranges { get; set; }

    public VertexArrayIndexed(IVertexBuffer[] vertex_buffers, IIndexBuffer index_buffer, List<DrawRange> draw_ranges): base(vertex_buffers) {
        /* foreach (var vb in vertex_buffers) {
            if (vb.length > index_buffer.max_vertex_count)
                throw new ArgumentOutOfRangeException(nameof(index_buffer), $"IndexBuffer<{index_buffer.item_type.Name}> too small for VertexBuffer(count={vb.length})!");
        }*/

        this.index_buffer = index_buffer;
        this.draw_ranges   = draw_ranges;
    }

    public VertexArrayIndexed(IVertexBuffer vertex_buffer, IIndexBuffer index_buffer, List<DrawRange> draw_ranges)
        : this([vertex_buffer], index_buffer, draw_ranges) {}

    public VertexArrayIndexed(IVertexBuffer vertex_buffer, IIndexBuffer index_buffer)
        : this([vertex_buffer], index_buffer,null!) {}

    public VertexArrayIndexed(IVertexBuffer[] vertex_buffers, IIndexBuffer index_buffer)
        : this(vertex_buffers, index_buffer, null!) {}

    public override void upload() {
        if (handle == 0)
            handle = GL.GenVertexArray();

        GL.BindVertexArray(handle);

        index_buffer.bind();

        upload_attribute_pointers();

        GL.BindVertexArray(0);

        Error.assert_opengl();

        //Console.WriteLine();
    }

    public override string ToString() {
        return $"vert:{vertex_buffers.sum(static buffer => buffer.length):N0}, ind:{index_buffer.length:N0}";
    }

    public override void draw() {
        //Console.WriteLine($"IndexedVertexArray.draw ({primitive_type}, {index_buffer.length * 3}, {index_buffer.draw_element_type}, 0)");
        if (draw_ranges == null || draw_ranges.Count <= 1) {
            GL.DrawElements(primitive_type, index_buffer.length * 3, index_buffer.draw_element_type, 0);
        } else {
            foreach (var dr in draw_ranges) {
                GL.DrawElementsBaseVertex(
                                          primitive_type,
                                          dr.draw_count * 3,
                                          index_buffer.draw_element_type,
                                          dr.first_index * 3,
                                          dr.base_vertex
                                         );
            }
        }

        Error.assert_opengl();
    }
}