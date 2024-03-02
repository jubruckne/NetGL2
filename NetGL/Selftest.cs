using System.Numerics;
using System.Runtime.CompilerServices;
using NetGL;
using Vector3 = OpenTK.Mathematics.Vector3;

public class Selftest {
    private static bool failed;

    public static bool run() {
        failed = false;
        TestRotation();
        return !failed;
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