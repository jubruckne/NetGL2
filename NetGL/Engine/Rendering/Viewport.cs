using OpenTK.Graphics.OpenGL;

namespace NetGL;

public class Viewport {
    private static Viewport? current_viewport = null;

    public int x;
    public int y;
    public int width;
    public int height;

    private bool resized = false;

    public void resize(int x, int y, int width, int height) {
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
        resized = true;
    }

    public void bind() {
        if (current_viewport != this || resized) {
            GL.Viewport(x, y, width, height);
            current_viewport = this;
            resized = false;
        }
    }
}