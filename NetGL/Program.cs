using OpenTK.Windowing.Common;

Console.WriteLine("Hello, World!");
Engine engine = new("NetGL", window_size:(1320, 200), window_state:WindowState.Maximized, debug:true);
engine.Run();