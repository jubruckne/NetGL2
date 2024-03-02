using NetGL;
using OpenTK.Windowing.Common;

Engine engine = new("NetGL", new Size2<int>(1024, 700), debug: true, window_state: WindowState.Maximized);

if (Selftest.run()) {
    engine.Run();
}