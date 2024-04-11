namespace NetGL;

using OpenTK.Graphics.OpenGL4;
using System.Collections;

public class VertexArrayIndexed: VertexArray {
    public class DrawRanges: IEnumerable<DrawRanges.DrawRange> {
        public readonly record struct DrawRange {
            public readonly int first_index;
            public readonly int last_index;
            public readonly int base_vertex;

            public int draw_count => last_index - first_index;

            internal DrawRange(int first_index, int last_index, int base_vertex) {
                this.first_index = first_index;
                this.last_index  = last_index;
                this.base_vertex = base_vertex;
            }

            public override string ToString()
                => $"<first={first_index}, last={last_index}, count={draw_count}, base_vertex={base_vertex}>";
        }

        public DrawRanges() {}

        public DrawRanges(DrawRanges other) => list.AddRange(other);

        public IntPtr[] indices = null!;
        public int[] counts = null!;
        public int[] base_vertices = null!;

        public void compile() {
            indices       = new IntPtr[length];
            counts        = new int[length];
            base_vertices = new int[length];

            for (int i = 0; i < length; i++) {
                indices[i]       = list[i].first_index;
                counts[i]        = list[i].draw_count;
                base_vertices[i] = list[i].base_vertex;
            }
        }

        private readonly List<DrawRange> list = [];
        public int length => list.Count;

        public void add(int first_index, int last_index, int base_vertex) {
            list.Add(new DrawRange(first_index, last_index, base_vertex));
        }

        public void clear() => list.Clear();

        IEnumerator IEnumerable.GetEnumerator() => list.GetEnumerator();
        IEnumerator<DrawRange> IEnumerable<DrawRange>.GetEnumerator()
            => list.GetEnumerator();
    }

    public readonly IIndexBuffer index_buffer;
    public readonly DrawRanges draw_ranges;

    public VertexArrayIndexed(List<IVertexBuffer> vertex_buffers, IIndexBuffer index_buffer, DrawRanges? draw_ranges, Material material): base(vertex_buffers, material) {
        this.index_buffer = index_buffer;

        if (draw_ranges != null && draw_ranges.length > 1) {
            this.draw_ranges = draw_ranges;
            this.draw_ranges.compile();
        } else {
            this.draw_ranges = new DrawRanges();
        }
    }

    public VertexArrayIndexed(IVertexBuffer vertex_buffer, IIndexBuffer index_buffer, DrawRanges? draw_ranges, Material material)
        : this([vertex_buffer], index_buffer, draw_ranges, material) {}

    public VertexArrayIndexed(IVertexBuffer vertex_buffer, IIndexBuffer index_buffer, Material material)
        : this([vertex_buffer], index_buffer,null, material) {}

    public VertexArrayIndexed(List<IVertexBuffer> vertex_buffers, IIndexBuffer index_buffer, Material material)
        : this(vertex_buffers, index_buffer, null, material) {}

    public override void upload() {
        if (handle == 0)
            handle = GL.GenVertexArray();

        GL.BindVertexArray(handle);

        index_buffer.bind();

        upload_attribute_pointers();

        GL.BindVertexArray(0);

        Debug.assert_opengl();

        //Console.WriteLine();
    }

    public override string ToString() {
        return $"vert:{vertex_buffers.sum(static buffer => buffer.length):N0}, ind:{index_buffer.length:N0}";
    }

    public override void draw_patches() {
        //Console.WriteLine($"IndexedVertexArray.draw ({primitive_type}, {index_buffer.length * 3}, {index_buffer.draw_element_type}, 0)");
        Debug.assert(false);
        GL.PatchParameter(PatchParameterInt.PatchVertices, 3);

        if (draw_ranges.length <= 1) {
            GL.DrawElements(PrimitiveType.Patches, index_buffer.length * 3, index_buffer.draw_element_type, 0);
        } else {
            GL.MultiDrawElementsBaseVertex(
                                           PrimitiveType.Patches,
                                           draw_ranges.counts,
                                           index_buffer.draw_element_type,
                                           draw_ranges.indices,
                                           draw_ranges.length,
                                           draw_ranges.base_vertices
                                          );
        }

        Debug.assert_opengl();
    }


    public override void draw() {
        //Console.WriteLine($"IndexedVertexArray.draw ({primitive_type}, {index_buffer.length * 3}, {index_buffer.draw_element_type}, 0)");
        if (draw_ranges.length <= 1) {
            GL.DrawElements(primitive_type, index_buffer.length * 3, index_buffer.draw_element_type, 0);
        } else {
            Debug.assert(false);

            GL.MultiDrawElementsBaseVertex(
                                           primitive_type,
                                           draw_ranges.counts,
                                           index_buffer.draw_element_type,
                                           draw_ranges.indices,
                                           draw_ranges.length,
                                           draw_ranges.base_vertices
                                           );
            /*
             Debug.println(draw_ranges, ConsoleColor.Yellow);
            foreach (var dr in draw_ranges) {
                GL.DrawElementsBaseVertex(
                                          primitive_type,
                                          dr.draw_count * 3,
                                          index_buffer.draw_element_type,
                                          dr.first_index * 3,
                                          dr.base_vertex
                                         );
            }*/
        }

        Debug.assert_opengl();
    }
}