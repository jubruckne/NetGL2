using NetGL;
using OpenTK.Windowing.Common;

Engine engine = new("NetGL", new Size2<int>(1024, 700), debugging: false, window_state: WindowState.Normal);

if (Selftest.run())
    engine.Run();