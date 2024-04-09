using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using NetGL.Vectors;
using Vector3 = System.Numerics.Vector3;

BenchmarkRunner.Run<EnhancedGraphicsBenchmark>(new VectorExtensions.QuietConfig());

[MemoryDiagnoser(true)]
public class EnhancedGraphicsBenchmark {
    public const int ParticleCount = 193;

    EnhancedGraphicsVec3Benchmark b1 = new EnhancedGraphicsVec3Benchmark();
    EnhancedGraphicsVec3Benchmark_sys_vec b2 = new EnhancedGraphicsVec3Benchmark_sys_vec();
    EnhancedGraphicsVec3Benchmark_sys_vec_t b3 = new EnhancedGraphicsVec3Benchmark_sys_vec_t();

    [GlobalSetup]
    public void Setup() {
        b3.Setup();

        b2.Setup();
        b1.Setup();

    }


    [Benchmark]
    public void UpdateParticlesWithPhysics_sys_vec_t() {
        b3.UpdateParticlesWithPhysics_vec_length();
    }

    [Benchmark]
    public void UpdateParticlesWithPhysics_vec() {
        b1.UpdateParticlesWithPhysics_vec_length();
    }
/*
    [Benchmark]
    public void UpdateParticlesWithPhysics_sys_vec() {
        b2.UpdateParticlesWithPhysics_vec();
    }
*/


    public class EnhancedGraphicsVec3Benchmark {
        private vec3<float>[] positions;
        private vec3<float>[] velocities;
        private vec3<float>[] forces;
        private readonly vec3<float> acceleration = new vec3<float>(0, -9.81f, 1f);

        public void Setup() {
            Random random = new Random(42);
            positions  = new vec3<float>[ParticleCount];
            velocities = new vec3<float>[ParticleCount];
            forces     = new vec3<float>[ParticleCount];

            for (int i = 0; i < ParticleCount; i++) {
                positions[i] = new vec3<float>(
                                               (float)random.NextDouble() * 100,
                                               (float)random.NextDouble() * 100,
                                               (float)random.NextDouble() * 100
                                              );
                velocities[i] = new vec3<float>(
                                                (float)random.NextDouble() * 10,
                                                (float)random.NextDouble() * 10,
                                                (float)random.NextDouble() * 10
                                               );
                forces[i] = new vec3<float>(
                                            (float)random.NextDouble() * 5,
                                            (float)random.NextDouble() * 5,
                                            (float)random.NextDouble() * 5
                                           );
            }
        }

        public void UpdateParticlesWithPhysics_vec_length() {
            float length = 0;

            for (var i = 0; i < ParticleCount * 750000; ++i) {
                length += vec.length(positions[i % 193] * 2f + velocities[i % 193]);
            }
        }

        public void UpdateParticlesWithPhysics_vec() {
            for (var x = 0; x < 1000; ++x) {
                for (var i = 0; i < ParticleCount; i++) {
                    // Apply a simple physics model: F = ma (assuming mass = 1 for simplicity)
                    var forceApplication = forces[i] * 0.016f; // Apply force with a time step
                    velocities[i] +=
                        acceleration + forceApplication; // Update velocity with acceleration and applied force

                    positions[i] += velocities[i]; // Update position with velocity

                    // Assume some interactions that require dot and cross products
                    var dotProduct   = vec.dot(positions[i], velocities[i]);
                    var crossProduct = vec.cross(positions[i], forces[i]);

                    positions[i] += crossProduct * dotProduct - velocities[i] / velocities[i].length();
                }
            }
        }
    }

    public class EnhancedGraphicsVec3Benchmark_sys_vec {
        private Vector3[] positions;
        private Vector3[] velocities;
        private Vector3[] forces;
        private readonly Vector3 acceleration = new(0, -9.81f, 1f);

        public void Setup() {
            Random random = new Random(42);
            positions  = new Vector3[ParticleCount];
            velocities = new Vector3[ParticleCount];
            forces     = new Vector3[ParticleCount];

            for (int i = 0; i < ParticleCount; i++) {
                positions[i] = new(
                                   (float)random.NextDouble() * 100,
                                   (float)random.NextDouble() * 100,
                                   (float)random.NextDouble() * 100
                                  );
                velocities[i] = new(
                                    (float)random.NextDouble() * 10,
                                    (float)random.NextDouble() * 10,
                                    (float)random.NextDouble() * 10
                                   );
                forces[i] = new(
                                (float)random.NextDouble() * 5,
                                (float)random.NextDouble() * 5,
                                (float)random.NextDouble() * 5
                               );
            }
        }

        public void UpdateParticlesWithPhysics_vec() {
            for (var x = 0; x < 1000; ++x) {
                for (var i = 0; i < ParticleCount; i++) {
                    // Apply a simple physics model: F = ma (assuming mass = 1 for simplicity)
                    var forceApplication = forces[i] * 0.016f; // Apply force with a time step
                    velocities[i] +=
                        acceleration + forceApplication; // Update velocity with acceleration and applied force

                    positions[i] += velocities[i]; // Update position with velocity

                    // Assume some interactions that require dot and cross products
                    var dotProduct   = Vector3.Dot(positions[i], velocities[i]);
                    var crossProduct = Vector3.Cross(positions[i], forces[i]);

                    positions[i] += crossProduct * dotProduct - velocities[i] / velocities[i].Length();
                }
            }
        }
    }

    public class EnhancedGraphicsVec3Benchmark_sys_vec_t {
        private Vector<float>[] positions;
        private Vector<float>[] velocities;
        private Vector<float>[] forces;
        private readonly Vector<float> acceleration = new Vector<float>([0, -9.81f, 1f, 0]);

        public void Setup() {
            Random random = new Random(42);
            positions  = new Vector<float>[ParticleCount];
            velocities = new Vector<float>[ParticleCount];
            forces     = new Vector<float>[ParticleCount];

            for (int i = 0; i < ParticleCount; i++) {
                positions[i] = new(
                                   [
                                       (float)random.NextDouble() * 100,
                                       (float)random.NextDouble() * 100,
                                       (float)random.NextDouble() * 100,
                                       0
                                   ]
                                  );
                velocities[i] = new(
                                    [
                                        (float)random.NextDouble() * 10,
                                        (float)random.NextDouble() * 10,
                                        (float)random.NextDouble() * 10,
                                        0
                                    ]
                                   );
                forces[i] = new(
                                [
                                    (float)random.NextDouble() * 5,
                                    (float)random.NextDouble() * 5,
                                    (float)random.NextDouble() * 5,
                                    0
                                ]
                               );
            }
        }

        public void UpdateParticlesWithPhysics_vec_length() {
            float length = 0;

            for (var i = 0; i < ParticleCount * 750000; ++i) {
                length += VectorExtensions.VectorLength(positions[i % 193] * 2f + velocities[i % 193]);
            }
        }

        public void UpdateParticlesWithPhysics_vec() {
            for (var x = 0; x < 1000; ++x) {
                for (var i = 0; i < ParticleCount; i++) {
                    // Apply a simple physics model: F = ma (assuming mass = 1 for simplicity)
                    var forceApplication = forces[i] * 0.016f; // Apply force with a time step
                    velocities[i] +=
                        acceleration + forceApplication; // Update velocity with acceleration and applied force

                    positions[i] += velocities[i]; // Update position with velocity

                    // Assume some interactions that require dot and cross products
                    var dotProduct   = Vector.Dot(positions[i], velocities[i]);
                    var crossProduct = VectorExtensions.Cross(positions[i], forces[i]);
                    positions[i] += crossProduct * dotProduct
                                    - velocities[i] / velocities[i].AsVector128().AsVector3().Length();
                }
            }
        }
    }
}

public static class VectorExtensions {
    public static Vector128<float> Cross(in Vector128<float> v1, in Vector128<float> v2) {
        Vector3 vv1    = new Vector3(v1.GetElement(0), v1.GetElement(1), v1.GetElement(2));
        Vector3 vv2    = new Vector3(v2.GetElement(0), v2.GetElement(1), v2.GetElement(2));
        Vector3 result = Vector3.Cross(vv1, vv2);
        return Vector128.Create(result.X, result.Y, result.Z, 0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector<float> Cross(in Vector<float> v1, in Vector<float> v2) {
        Vector3 vv1    = new Vector3(v1.GetElement(0), v1.GetElement(1), v1.GetElement(2));
        Vector3 vv2    = new Vector3(v2.GetElement(0), v2.GetElement(1), v2.GetElement(2));
        Vector3 result = Vector3.Cross(vv1, vv2);
        return new Vector<float>([result.X, result.Y, result.Z, 0]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float VectorLength(in Vector<float> vector) {
        return MathF.Sqrt((float)Vector.Dot<float>(vector, vector));
    }



    public class QuietConfig: ManualConfig {
        public QuietConfig() {
            // Use a minimal logger to reduce output verbosity
            AddLogger(ConsoleLogger.Default);

            // Add only the necessary columns to reduce information overload
            AddColumnProvider(DefaultColumnProviders.Instance);

            // Add minimal exporter to control output format (Markdown, PlainText, etc.)
            AddExporter(MarkdownExporter.GitHub);

            // Optional: Add diagnosers if needed, but be selective
            AddDiagnoser(MemoryDiagnoser.Default);

            // Adjust verbosity
            WithSummaryStyle(SummaryStyle.Default.WithMaxParameterColumnWidth(20));

        }
    }
}

public static class SimplexNoiseSIMD {
    // Gradient for 2D noise
    private static readonly int[][] grad2 = [
        [1, 1], [-1, 1], [1, -1], [-1, -1],
        [1, 0], [-1, 0], [1, 0], [-1, 0],
        [0, 1], [0, -1], [0, 1], [0, -1]
    ];

    // Permutation table.
    private static readonly int[] perm = [
        151, 160, 137, 91, 90, 15, // ... populate as before
        151, 160, 137, 91, 90, 15 // Duplicate to avoid overflow
    ];
    static readonly float F2 = 0.5f * ((float)Math.Sqrt(3.0) - 1.0f);
    static readonly float G2 = (3.0f - (float)Math.Sqrt(3.0)) / 6.0f;

    public static float GenerateSIMD(float xin, float yin) {
        var s = (xin + yin) * F2;
        var i = (int)MathF.Floor(xin + s);
        var j = (int)MathF.Floor(yin + s);
        var t = (i + j) * G2;
        var X0 = i - t;
        var Y0 = j - t;
        var x0 = xin - X0;
        var y0 = yin - Y0;

        int i1, j1;
        if (x0 > y0) { i1 = 1; j1 = 0; }
        else { i1 = 0; j1 = 1; }

        var x1 = x0 - i1 + G2;
        var y1 = y0 - j1 + G2;
        var x2 = x0 - 1.0f + 2.0f * G2;
        var y2 = y0 - 1.0f + 2.0f * G2;

        var ii = i & 255;
        var jj = j & 255;
        var gi0 = perm[ii + perm[jj]] % 12;
        var gi1 = perm[ii + i1 + perm[jj + j1]] % 12;
        var gi2 = perm[ii + 1 + perm[jj + 1]] % 12;

        var g0 = Vector64.Create(grad2[gi0][0], (float)grad2[gi0][1]);
        var g1 = Vector64.Create(grad2[gi1][0], (float)grad2[gi1][1]);
        var g2 = Vector64.Create(grad2[gi2][0], (float)grad2[gi2][1]);

        var d0 = Vector64.Create(x0, y0);
        var d1 = Vector64.Create(x1, y1);
        var d2 = Vector64.Create(x2, y2);

        var n0 = Vector64.Dot(g0, d0);
        var n1 = Vector64.Dot(g1, d1);
        var n2 = Vector64.Dot(g2, d2);

        // Compute contribution factors
        n0 = n0 > 0 ? 70.0f * (n0 * n0) * (n0 * n0) : 0.0f;
        n1 = n1 > 0 ? 70.0f * (n1 * n1) * (n1 * n1) : 0.0f;
        n2 = n2 > 0 ? 70.0f * (n2 * n2) * (n2 * n2) : 0.0f;

        // The result is scaled to return values in the interval [-1,1].
        return n0 + n1 + n2;
    }
}