/*using System.Numerics;
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
#1#


}*/

Console.WriteLine("Hello, World!");