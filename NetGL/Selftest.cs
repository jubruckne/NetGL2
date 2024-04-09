using System.Runtime.CompilerServices;
using NetGL;
using OpenTK.Mathematics;
using Vector3 = OpenTK.Mathematics.Vector3;

public static class Selftest {
    private static bool failed;

    public static bool run() {
        failed = false;
        TestRotation();
        TestPathfinder();
        TestMemoryPool1();
        TestMemoryPool2();
        GC.AddMemoryPressure(9999999);
        GC.Collect(3, GCCollectionMode.Forced, true, true);
        GC.AddMemoryPressure(9999999);

        GC.Collect(2, GCCollectionMode.Forced, true, true);
        GC.AddMemoryPressure(9999999);
        GC.Collect(1, GCCollectionMode.Forced, true, true);
        GC.AddMemoryPressure(9999999);
        GC.Collect(0, GCCollectionMode.Default, true, true);
        GC.AddMemoryPressure(9999999);
        GC.Collect(1, GCCollectionMode.Default, true, true);
        GC.AddMemoryPressure(9999999);
        GC.Collect(2, GCCollectionMode.Default, true, true);
        GC.AddMemoryPressure(9999999);
        TestMemoryPool3();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        return !failed;
    }

    private static void TestMemoryPool1() {
        Pointer.on_allocate += static pointer
            => Console.WriteLine($"allocating: {pointer}, still alive: {Pointer.alive_count}");
        Pointer.on_release += static pointer
            => Console.WriteLine($"releasing: {pointer}, still alive: {Pointer.alive_count}");

        var v = Pool<Vector3>.allocate();
        Console.WriteLine("TestMemoryPool1 = " + v);

        Console.WriteLine(Pool<Vector3>.capacity);
        Console.WriteLine(Pool<Vector3>.used_capacity);
    }

    private static void TestMemoryPool2() {
        var v = Pool<Vector3>.allocate();
        Console.WriteLine("TestMemoryPool2 = " + v);

        Console.WriteLine(Pool<Vector3>.capacity);
        Console.WriteLine(Pool<Vector3>.used_capacity);
    }

    private static void TestMemoryPool3() {
        Console.WriteLine(Pool<Vector3>.capacity);
        Console.WriteLine(Pool<Vector3>.used_capacity);
        Console.WriteLine(Pointer.alive_count);
    }

    private static void TestPathfinder() {
        int[,] maze = {
            { 0, 0, 1, 0, 0 },
            { 1, 0, 1, 0, 1 },
            { 1, 0, 0, 0, 0 },
            { 0, 1, 1, 1, 0 },
            { 0, 0, 0, 0, 0 }
        };

        var directions = new List<(int, int)> { (0, -1), (1, 0), (0, 1), (-1, 0) }; // Up, Right, Down, Left

        IEnumerable<(Vector2i neighbor, float cost)> get_neighbors(Vector2i node) {
            foreach (var (dx, dy) in directions) {
                int newX = node.X + dx;
                int newY = node.Y + dy;
                if (newX >= 0 && newX < maze.GetLength(0) && newY >= 0 && newY < maze.GetLength(1) && maze[newX, newY] == 0) {
                    yield return ((newX, newY), 1f);
                }
            }
        }

        var pathfinder = new Pathfinder<Vector2i>(
            heuristic: static (from, to) => (from - to).ManhattanLength,
            neighbors:  get_neighbors
        );

        if (pathfinder.find_path((0, 0), (4, 4), out var path)) {
            Console.WriteLine("Found Path");
            foreach (var p in path) {
                Console.WriteLine(p);
            }
        } else {
            Console.WriteLine("not found!");
            assert(false, true);
        }


    }

    private static void TestRotation() {
        var r = Rotation.Forward;

        Console.WriteLine(r);
        assert(r.forward, -Vector3.UnitZ);
        assert(r.back, Vector3.UnitZ);

        assert(r.right, Vector3.UnitX);
        assert(r.left, -Vector3.UnitX);

        assert(r.up, Vector3.UnitY);
        assert(r.down, -Vector3.UnitY);

        // ---------- back -----------
        r = Rotation.Back;
        Console.WriteLine(r);
        assert(r.forward, Vector3.UnitZ);
        assert(r.back, -Vector3.UnitZ);

        assert(r.right, -Vector3.UnitX);
        assert(r.left, Vector3.UnitX);

        assert(r.up, Vector3.UnitY);
        assert(r.down, -Vector3.UnitY);

        // ---------- right -----------
        r = Rotation.Right;
        Console.WriteLine(r);
        assert(r.forward, Vector3.UnitX);
        assert(r.back, -Vector3.UnitX);

        assert(r.right, Vector3.UnitZ);
        assert(r.left, -Vector3.UnitZ);

        assert(r.up, Vector3.UnitY);
        assert(r.down, -Vector3.UnitY);

        // ------------ up -------------
        r = Rotation.Up;
        Console.WriteLine(r);
        assert(r.forward, Vector3.UnitY);
        assert(r.back, -Vector3.UnitY);

        assert(r.right, Vector3.UnitX);
        assert(r.left, -Vector3.UnitX);

        assert(r.up, Vector3.UnitZ);
        assert(r.down, -Vector3.UnitZ);

    }

    private static void assert<T>(
        in T left, in T right,
        [CallerArgumentExpression("left")] string? l_expr = null,
        [CallerArgumentExpression("right")] string? r_expr = null)
    where T: struct, IEquatable<T> {
        if (!left.Equals(right)) {
            failed = true;
            Console.WriteLine($"[green]{l_expr} [default]== [blue]{r_expr} [default]---> [green]{left} [default]!= [blue]{right}!");
        }
    }
}