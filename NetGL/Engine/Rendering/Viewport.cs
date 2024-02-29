using OpenTK.Graphics.OpenGL;

namespace NetGL;

public class Viewport {
    private static Viewport? current_viewport;

    public readonly string name;

    public int x;
    public int y;
    public int width;
    public int height;

    private bool resized = false;

    public Viewport(string name, int x, int y, int width, int height, Color clear_color) {
        this.name = name;
        resize(x, y, width, height);
        this.clear_color = clear_color;
    }

    public Viewport(string name, int x, int y, int width, int height): this(name, x, y, width, height, Color.Black) { }

    public static Viewport Gameplay = new("Gameplay", 0, 0, 800, 600, Color.Black);
    public static Viewport Hud = new("Hud", 1000, 800, 400, 300, new Color(20, 20, 25, 255));

    public int left => x;
    public int right => x + width;
    public int top => y + height;
    public int bottom => y;

    private Color _clear_color;

    public Color clear_color {
        get => _clear_color;
        set {
            _clear_color = value;
            if(is_current)
                GL.ClearColor(_clear_color.reinterpret_ref<Color, OpenTK.Mathematics.Color4>());
        }
    }

    public void resize(int x, int y, int width, int height) {
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
        resized = true;
    }

    public void clear() {
        if (!is_current) throw new Error.WrongContextException(ToString(), current_viewport?.ToString() ?? "None");
        if (this == Gameplay) {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        } else {
            GL.Enable(EnableCap.ScissorTest);
            GL.Scissor(x, y, width, height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Disable(EnableCap.ScissorTest);
        }
    }

    public void make_current() {
        if (current_viewport != this || resized) {
            GL.Viewport(x, y, width, height);
            GL.ClearColor(_clear_color.reinterpret_ref<Color, OpenTK.Mathematics.Color4>());

            current_viewport = this;
            resized = false;
        }
    }

    public bool is_current => current_viewport == this;

    public override string ToString() {
        return $"{name}(x:{x}, y:{y}, w:{width}, h:{height}, color:{_clear_color}";
    }

    public Viewport copy(in string name_of_copy, int? x=null, int? y=null, int? width=null, int? height=null) {
        return new Viewport(name_of_copy, x ?? this.x, y ?? this.y, width ?? this.width, height ?? this.height, _clear_color);
    }
}