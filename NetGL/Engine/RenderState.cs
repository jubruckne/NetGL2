using System.Diagnostics.CodeAnalysis;

namespace NetGL;

using ECS;
using System.Reflection;
using OpenTK.Graphics.OpenGL4;

public interface IState: INamed {
    object value { get; }
}

public abstract class State<T>: IState
    where T: IEquatable<T> {

    public string name { get; }
    private T state;
    private readonly bool write_through;
    private readonly Stack<T> stack = new();

    protected abstract void set_state(T state);
    protected abstract T get_state();

    object IState.value => this.value;

    public T value {
        get => this.state;
        [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract")]
        set {
            if (this.state == null && value == null) return;

            if (this.state != null && this.state.Equals(value)) {
                ++RenderState.state_changes_avoided;
                return;
            }

            ++RenderState.state_changes_count;
            this.state = value;
            if(write_through) set_state(value);
        }
    }

    protected State(T state, bool write_through) {
        name = this.get_type_name();
        this.write_through = write_through;
        this.state         = state;
        this.value         = state;
    }

    [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract")]
    public bool verify() {
        if (get_state() is null)
            return state is null;

        return get_state().Equals(state);
    }

    public void assert() {
        if (!verify())
            Error.exception($"State '{name}' should be <{state}> but readback says <{get_state()}>");
    }

    public override string ToString() => $"{this.get_type_name()}={value}";

    public static explicit operator T(State<T> render_state) => render_state.value;

    public void push(in T state) {
        stack.Push(value);
        value = state;
    }

    public void pop() {
        value = stack.Pop();
    }
}

public static partial class RenderState {
    public static (int state_changes_count, int state_changes_avoided) statistics => (state_changes_count, state_changes_avoided);

    [SuppressMessage("ReSharper", "StaticMemberInGenericType")] internal static int state_changes_count = 0;
    [SuppressMessage("ReSharper", "StaticMemberInGenericType")] internal static int state_changes_avoided = 0;

    public static readonly State<NetGL.Shader> shader = create<RenderState.Shader>();
    public static readonly State<bool> depth_test = create<RenderState.DepthTest>();
    public static readonly State<bool> cull_face = create<RenderState.CullFace>();
    public static readonly State<bool> blending = create<RenderState.Blending>();
    public static readonly State<bool> wireframe = create<RenderState.Wireframe>();
    public static readonly State<bool> front_facing = create<RenderState.FrontFacing>();
    public static readonly State<bool> scissor_test = create<RenderState.ScissorTest>();

    private static T create<T>() {
        var type = typeof(T);

        var ctor = type.GetConstructor(
                                       BindingFlags.Instance | BindingFlags.NonPublic,
                                       null,
                                       [],
                                       null
                                      );

        if (ctor != null)
            return (T)ctor.Invoke(null);

        Error.exception($"Can not construct type {type.get_type_name()}");
        return default;
    }

    public static T get<T>(this Bag<IState> states) {
        foreach (var state in states) {
            if (state is T t) return t;
        }

        Error.index_out_of_range(states);
        return default!;
    }

    public static void bind(in NetGL.Shader shader) {
        RenderState.shader.value = shader;
    }

    public static void bind(in RenderSettings settings) {
        depth_test.value   = settings.depth_test;
        cull_face.value    = settings.cull_face;
        blending.value     = settings.blending;
        wireframe.value    = settings.wireframe;
        front_facing.value = settings.front_facing;
        scissor_test.value = settings.scissor_test;
    }

    public static Bag<IState> bind(this Bag<IState> states) {
        foreach (var state in states) {
            switch (state) {
                case RenderState.CullFace cf:
                    RenderState.cull_face.value = cf.value;
                    break;
                case RenderState.DepthTest dt:
                    RenderState.depth_test.value = dt.value;
                    break;
                case RenderState.FrontFacing ff:
                    RenderState.front_facing.value = ff.value;
                    break;
                case Shader sh:
                    RenderState.shader.value = sh.value;
                    break;
                case RenderState.Wireframe wf:
                    RenderState.wireframe.value = wf.value;
                    break;
                case RenderState.Blending bl:
                    RenderState.blending.value = bl.value;
                    break;
                case RenderState.ScissorTest st:
                    RenderState.scissor_test.value = st.value;
                    break;
                default:
                    Error.index_out_of_range(state);
                    break;
            }
        }

        return states;
    }

    public static NetGL.Shader bind(State<NetGL.Shader> render_state, in NetGL.Shader shader) {
        render_state.value = shader;
        return shader;
    }

    public static void toggle(this State<bool> render_state) =>
        render_state.value = !render_state.value;

    public static void enable(this State<bool> render_state) =>
        render_state.value = true;

    public static void disable(this State<bool> render_state) =>
        render_state.value = false;

    public static void assert() {
        shader.assert();
        depth_test.assert();
        cull_face.assert();
        blending.assert();
        wireframe.assert();
        front_facing.assert();
        scissor_test.assert();
    }
}

public static partial class RenderState {
    public class DepthTest: State<bool> {
        private DepthTest(): base(true, true) {}
        public DepthTest(bool state): base(state, false) {}

        protected override void set_state(bool state) {
            if (state) GL.Enable(EnableCap.DepthTest);
            else GL.Disable(EnableCap.DepthTest);
        }

        protected override bool get_state() => GL.GetBoolean(GetPName.DepthTest);
    }

    public class CullFace: State<bool> {
        private CullFace(): base(true, true) {}
        public CullFace(bool state): base(state, false) {}

        protected override bool get_state() => GL.GetBoolean(GetPName.CullFace);

        protected override void set_state(bool state) {
            if (state) GL.Enable(EnableCap.CullFace);
            else GL.Disable(EnableCap.CullFace);
        }
    }

    public class ScissorTest: State<bool> {
        private ScissorTest(): base(GL.GetBoolean(GetPName.ScissorTest), true) {}
        public ScissorTest(bool state): base(state, false) {}

        protected override bool get_state() => GL.GetBoolean(GetPName.ScissorTest);

        protected override void set_state(bool state) {
            if (state) GL.Enable(EnableCap.ScissorTest);
            else GL.Disable(EnableCap.ScissorTest);
        }
    }

    public class Blending: State<bool> {
        private Blending(): base(false, true) {}
        public Blending(bool state): base(state, false) {}

        protected override bool get_state() => GL.GetBoolean(GetPName.Blend);

        protected override void set_state(bool state) {
            if (state) GL.Enable(EnableCap.Blend);
            else GL.Disable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
        }
    }

    public class Wireframe: State<bool> {
        private Wireframe(): base(false, true) {}
        public Wireframe(bool state): base(state, false) {}

        protected override bool get_state() => GL.GetInteger(GetPName.PolygonMode) == (int)MaterialFace.FrontAndBack;

        protected override void set_state(bool state)
            => GL.PolygonMode(MaterialFace.FrontAndBack, state ? PolygonMode.Line : PolygonMode.Fill);
    }

    public class FrontFacing: State<bool> {
        private FrontFacing(): base(true, true) {}
        public FrontFacing(bool state): base(state, false) {}

        protected override bool get_state() => GL.GetInteger(GetPName.FrontFace) == (int)FrontFaceDirection.Ccw;

        protected override void set_state(bool state) =>
            GL.FrontFace(state ? FrontFaceDirection.Ccw : FrontFaceDirection.Cw);
    }

    public class Shader: State<NetGL.Shader> {
        private Shader(): base(null!, true) {}
        public Shader(NetGL.Shader shader): base(shader, false) {}

        protected override void set_state(NetGL.Shader shader) {
            GL.UseProgram(shader.handle);
        }

        protected override NetGL.Shader get_state() {
            var current = GL.GetInteger(GetPName.CurrentProgram);
            if (NetGL.Shader.instances.TryGetValue(current, out var sh)) {
                if (sh.TryGetTarget(out var shader)) {
                    return shader;
                }
            }
            return null!;
        }
    }
}