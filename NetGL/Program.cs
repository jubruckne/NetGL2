using OpenTK.Windowing.Common;

Engine engine = new("NetGL", (1280, 820), debugging: true, window_state: WindowState.Normal);

//if (Selftest.run())
engine.Run();