using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

namespace NetGL;

[SkipLocalsInit]
public readonly struct SimplexKernel_SIMD: IKernel {
    private const int X = 501125321;
    private const int Y = 1136930381;
    private const float SQRT3 = 1.7320508075688772935274463415059f;
    private const float F2    = 0.5f * (SQRT3 - 1.0f);
    private const float G2    = (3.0f - SQRT3) / 6.0f;
    private const float ROOT2 = 1.4142135623730950488f;
    private const float ROOT2_PLUS_1 = 1.0f + ROOT2;

    private static readonly Vector128<float> vec4_float_0_5 = Vector128.Create(0.5f);
    private static readonly Vector128<float> vec4_float_F2 = Vector128.Create(F2);
    private static readonly Vector128<float> vec4_float_G2 = Vector128.Create(G2);
    private static readonly Vector128<float> vec4_float_G2_times_2_minus_1 = Vector128.Create(G2 * 2f - 1f);
    private static readonly Vector128<float> vec4_float_root2_plus_1 = Vector128.Create(ROOT2_PLUS_1);
    private static readonly Vector128<int> vec4_int_prime_X = Vector128.Create(X);
    private static readonly Vector128<int> vec4_int_prime_Y = Vector128.Create(Y);
    private static readonly Vector128<float> vec4_float_38 = Vector128.Create(38.283687591552734375f);

    private static readonly Vector128<int> seed = Vector128.Create(1337);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static Vector128<float> IKernel.evaluate(Vector128<float> x, Vector128<float> y) {
        // float32v f  = float32v( F2 ) * (x + y);
        var f = vec4_float_F2 * (x + y);

        // float32v x0 = FS_Floor_f32( x + f );
        var x0 = Vector128.Floor(x + f);

        // float32v y0 = FS_Floor_f32( y + f );
        var y0 = Vector128.Floor(y + f);

        // int32v i = FS_Convertf32_i32( x0 ) * int32v( FnPrimes::X );
        var i = Vector128.ConvertToInt32(x0) * vec4_int_prime_X;

        // int32v j = FS_Convertf32_i32( y0 ) * int32v( FnPrimes::Y );
        var j = Vector128.ConvertToInt32(y0) * vec4_int_prime_Y;


        // float32v g = float32v( G2 ) * (x0 + y0);
        var g = vec4_float_G2 * (x0 + y0);

        // x0 = x - (x0 - g);
        x0 = x - (x0 - g);

        // y0 = y - (y0 - g);
        y0 = y - (y0 - g);


        // mask32v i1 = x0 > y0;
        var i1 = AdvSimd.CompareGreaterThan(x0, y0);

        // float32v x1 = FS_MaskedSub_f32( x0, float32v( 1.f ), i1 ) + float32v( G2 );
        var x1 = masked_sub_f32(x0, Vector128<float>.One, i1)
                 + vec4_float_G2;

        // float32v y1 = FS_NMaskedSub_f32( y0, float32v( 1.f ), i1 ) + float32v( G2 );
        var y1 = masked_sub_f32(y0, Vector128<float>.One, AdvSimd.Not(i1))
                 + vec4_float_G2;


        // float32v x2 = x0 + float32v( G2 * 2 - 1 );
        var x2 = x0 + vec4_float_G2_times_2_minus_1;

        // float32v y2 = y0 + float32v( G2 * 2 - 1 );
        var y2 = y0 + vec4_float_G2_times_2_minus_1;


        // float32v t0 = FS_FNMulAdd_f32( x0, x0, FS_FNMulAdd_f32( y0, y0, float32v( 0.5f ) ) );
        var t0 = n_mul_add_f32(x0, x0, n_mul_add_f32(y0, y0, vec4_float_0_5));

        // float32v t1 = FS_FNMulAdd_f32( x1, x1, FS_FNMulAdd_f32( y1, y1, float32v( 0.5f ) ) );
        var t1 = n_mul_add_f32(x1, x1, n_mul_add_f32(y1, y1, vec4_float_0_5));

        // float32v t2 = FS_FNMulAdd_f32( x2, x2, FS_FNMulAdd_f32( y2, y2, float32v( 0.5f ) ) );
        var t2 = n_mul_add_f32(x2, x2, n_mul_add_f32(y2, y2, vec4_float_0_5));

        // t0 = FS_Max_f32( t0, float32v( 0 ) );
        t0 = Vector128.Max(t0, Vector128<float>.Zero);

        // t1 = FS_Max_f32( t1, float32v( 0 ) );
        t1 = Vector128.Max(t1, Vector128<float>.Zero);

        // t2 = FS_Max_f32( t2, float32v( 0 ) );
        t2 = Vector128.Max(t2, Vector128<float>.Zero);

        // t0 *= t0; t0 *= t0;
        t0 *= t0;
        t0 *= t0;

        // t1 *= t1; t1 *= t1;
        t1 *= t1;
        t1 *= t1;

        // t2 *= t2; t2 *= t2;
        t2 *= t2;
        t2 *= t2;

        // float32v n0 = FnUtils::GetGradientDot( FnUtils::HashPrimes( seed, i, j ), x0, y0 );
        var n0 = get_gradient_dot(hash_primes(i, j), x0, y0);

        // float32v n1 = FnUtils::GetGradientDot(
        //      FnUtils::HashPrimes(
        //          seed,
        //          FS_MaskedAdd_i32(
        //              i,
        //              int32v( FnPrimes::X ), i1 ),
        //              FS_NMaskedAdd_i32(
        //                  j,
        //                  int32v( FnPrimes::Y ),
        //                  i1
        //              )
        //          ),
        //          x1,
        //          y1
        //      );
        var n1 = get_gradient_dot(
                                  hash_primes(
                                              masked_add_i32(
                                                             i,
                                                             vec4_int_prime_X,
                                                             i1.As<float, int>()
                                                            ),
                                              masked_n_add_i32(
                                                               j,
                                                               vec4_int_prime_Y,
                                                               i1.As<float, int>()
                                                              )
                                             ),
                                  x1,
                                  y1
                                 );

        // float32v n2 = FnUtils::GetGradientDot(
        //      FnUtils::HashPrimes(
        //          seed,
        //          i + int32v( FnPrimes::X ),
        //          j + int32v( FnPrimes::Y )
        //      ),
        //      x2,
        //      y2
        // );
        var n2 = get_gradient_dot(
                                  hash_primes(
                                              i + vec4_int_prime_X,
                                              j + vec4_int_prime_Y
                                             ),
                                  x2,
                                  y2
                                 );

        // return float32v( 38.283687591552734375f )
        //              * FS_FMulAdd_f32(
        //                      n0,
        //                      t0,
        //                      FS_FMulAdd_f32(
        //                          n1,
        //                          t1,
        //                          n2 * t2
        //                      )
        // );

        return
            vec4_float_38 * AdvSimd.FusedMultiplyAdd(
                                                     AdvSimd.FusedMultiplyAdd(
                                                                              n0 * t0,
                                                                              n1,
                                                                              t1
                                                                             ),
                                                     n2,
                                                     t2
                                                    );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<int> masked_add_i32(Vector128<int> left, Vector128<int> right, Vector128<int> mask) {
        var add = Vector128.ConditionalSelect(mask, right, Vector128<int>.Zero);
        return left + add;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<int> masked_n_add_i32(Vector128<int> left, Vector128<int> right, Vector128<int> mask) {
        var add = Vector128.ConditionalSelect(AdvSimd.Not(mask), right, Vector128<int>.Zero);
        return left + add;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<float> masked_sub_f32(Vector128<float> left, Vector128<float> right, Vector128<float> mask) {
        //         return a - FS::Mask_f32( b, m );
        var sub = Vector128.ConditionalSelect(mask, right, Vector128<float>.Zero);
        return left - sub;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<float> n_mul_add_f32(Vector128<float> a, Vector128<float> b, Vector128<float> c )
        => -(a * b) + c;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<int> hash_primes(Vector128<int> i, Vector128<int> j) {
        var hash = seed;
        hash ^= i;
        hash ^= j;
        hash *= 0x27d4eb2d;
        return (hash >> 15) ^ hash;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<float> get_gradient_dot(Vector128<int> hash, Vector128<float> fX, Vector128<float> fY) {
        // ( 1+R2, 1 ) ( -1-R2, 1 ) ( 1+R2, -1 ) ( -1-R2, -1 )
        // ( 1, 1+R2 ) ( 1, -1-R2 ) ( -1, 1+R2 ) ( -1, -1-R2 )

        // int32v  bit1 = (hash << 31);
        var bit1 = hash << 31;

        // int32v  bit2 = (hash >> 1) << 31;
        var bit2 = (hash >> 1) << 31;

        var bit4 = ((hash << 29) >> 31).As<int, float>();

        // fX ^= FS_Casti32_f32( bit1 );
        fX ^= bit1.As<int, float>();

        // fY ^= FS_Casti32_f32( bit2 );
        fY ^= bit2.As<int, float>();

        // float32v a = FS_Select_f32( bit4, fY, fX );
        var a = Vector128.ConditionalSelect(bit4, fY, fX);

        // float32v b = FS_Select_f32( bit4, fX, fY );
        var b = Vector128.ConditionalSelect(bit4, fX, fY);

        //return FS_FMulAdd_f32( float32v( 1.0f + ROOT2 ), a, b );
        return AdvSimd.FusedMultiplyAdd(b, a, vec4_float_root2_plus_1);
    }
}