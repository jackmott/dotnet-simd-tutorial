
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics;
using System.Runtime.CompilerServices;

namespace SIMDTutorial
{
    public static class SIMDHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Vector128<float> LoadFloatArray128(float[] arr, int i)
        {
            fixed (float* ptr = &arr[i])
            {
                return Sse.LoadVector128(ptr);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void StoreFloatArray128(float[] arr, int i, Vector128<float> value)
        {
            fixed (float* ptr = &arr[i])
            {
                Sse.Store(ptr, value);
            }
        }

        public static unsafe Vector256<float> LoadFloatArray256(float[] arr, int i)
        {
            fixed (float* ptr = &arr[i])
            {
                return Avx.LoadVector256(ptr);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void StoreFloatArray256(float[] arr, int i, Vector256<float> value)
        {
            fixed (float* ptr = &arr[i])
            {
                Avx.Store(ptr, value);
            }
        }
    }
    public struct Vectors3
    {
        public float[] x;
        public float[] y;
        public float[] z;

        public Vectors3(int count)
        {
            x = new float[count];
            y = new float[count];
            z = new float[count];
        }

        public void Add(Vectors3 v)
        {
            unsafe
            {
                fixed (float*
                      ax = &this.x[0],
                      ay = &this.y[0],
                      az = &this.z[0],
                      bx = &v.x[0],
                      by = &v.y[0],
                      bz = &v.z[0])
                {
                    for (int i = 0; i < this.x.Length; i++)
                    {
                        ax[i] += bx[i];
                        ay[i] += by[i];
                        az[i] += bz[i];
                    }
                }
            }
        }

        public void SseAdd(Vectors3 v)
        {
            unsafe
            {
                fixed (float*
                    ax = &this.x[0],
                    ay = &this.y[0],
                    az = &this.z[0],
                    bx = &v.x[0],
                    by = &v.y[0],
                    bz = &v.z[0])
                {
                    for (int i = 0; i < this.x.Length; i = i + 4)
                    {
                        var axV = Sse.LoadVector128(&ax[i]);
                        var ayV = Sse.LoadVector128(&ay[i]);
                        var azV = Sse.LoadVector128(&az[i]);

                        var bxV = Sse.LoadVector128(&bx[i]);
                        var byV = Sse.LoadVector128(&by[i]);
                        var bzV = Sse.LoadVector128(&bz[i]);

                        Sse.Store(&ax[i], Sse.Add(axV, bxV));
                        Sse.Store(&ay[i], Sse.Add(ayV, byV));
                        Sse.Store(&az[i], Sse.Add(azV, bzV));

                    }

                }
            }
        }


        public void Norm()
        {
            for (int i = 0; i < this.x.Length; i++)
            {
                var len = System.MathF.Sqrt(this.x[i] * this.x[i] + this.y[i] * this.y[i] + this.z[i] * this.z[i]);
                this.x[i] /= len;
                this.y[i] /= len;
                this.z[i] /= len;
            }
        }

        public void SseNorm()
        {
            for (int i = 0; i < this.x.Length; i = i + 4)
            {
                unsafe
                {
                    Vector128<float> ax = SIMDHelper.LoadFloatArray128(this.x, i);
                    Vector128<float> ay = SIMDHelper.LoadFloatArray128(this.y, i);
                    Vector128<float> az = SIMDHelper.LoadFloatArray128(this.z, i);

                    // len = 1/sqrt(x*x+y*y+z*z)
                    var len = Sse.ReciprocalSqrt(Sse.Add(Sse.Add(Sse.Multiply(ax, ax), Sse.Multiply(ay, ay)), Sse.Multiply(az, az)));

                    SIMDHelper.StoreFloatArray128(this.x, i, Sse.Multiply(ax, len));
                    SIMDHelper.StoreFloatArray128(this.y, i, Sse.Multiply(ay, len));
                    SIMDHelper.StoreFloatArray128(this.z, i, Sse.Multiply(az, len));
                }
            }

        }

        public void Clamp(float min)
        {
            for (int i = 0; i < this.x.Length; i++)
            {
                var len = System.MathF.Sqrt(this.x[i] * this.x[i] + this.y[i] * this.y[i] + this.z[i] * this.z[i]);
                if (len < min)
                {
                    len = (1.0f / len) * min;
                    this.x[i] *= len;
                    this.y[i] *= len;
                    this.z[i] *= len;
                }
            }
        }

        public void SseClamp(float min)
        {
            var trueResult = Vector128.Create(min);
            var falseResult = Vector128.Create(1.0f);

            for (int i = 0; i < this.x.Length; i = i + 4)
            {
                Vector128<float> ax = SIMDHelper.LoadFloatArray128(this.x, i);
                Vector128<float> ay = SIMDHelper.LoadFloatArray128(this.y, i);
                Vector128<float> az = SIMDHelper.LoadFloatArray128(this.z, i);

                // len = sqrt(x*x+y*y+z*z)
                var len = Sse.Sqrt(Sse.Add(Sse.Add(Sse.Multiply(ax, ax), Sse.Multiply(ay, ay)), Sse.Multiply(az, az)));
                var mask = Sse.CompareLessThan(len, trueResult);
                var result = Sse.Or(Sse.And(mask, trueResult), Sse.AndNot(mask, falseResult));

                SIMDHelper.StoreFloatArray128(this.x, i, Sse.Multiply(Sse.Divide(ax, len), result));
                SIMDHelper.StoreFloatArray128(this.y, i, Sse.Multiply(Sse.Divide(ay, len), result));
                SIMDHelper.StoreFloatArray128(this.z, i, Sse.Multiply(Sse.Divide(az, len), result));

            }
        }

        public void AvxClamp(float min)
        {
            var trueResult = Vector256.Create(min);
            var falseResult = Vector256.Create(1.0f);

            for (int i = 0; i < this.x.Length; i = i + 8)
            {
                Vector256<float> ax = SIMDHelper.LoadFloatArray256(this.x, i);
                Vector256<float> ay = SIMDHelper.LoadFloatArray256(this.y, i);
                Vector256<float> az = SIMDHelper.LoadFloatArray256(this.z, i);

                // len = sqrt(x*x+y*y+z*z)
                var len = Avx.Sqrt(Avx.Add(Avx.Add(Avx.Multiply(ax, ax), Avx.Multiply(ay, ay)), Avx.Multiply(az, az)));
                Vector256<float> mask = Avx.Compare(len, trueResult, FloatComparisonMode.OrderedLessThanSignaling);
                var result = Avx.BlendVariable(falseResult, trueResult, mask);

                SIMDHelper.StoreFloatArray256(this.x, i, Avx.Multiply(Avx.Divide(ax, len), result));
                SIMDHelper.StoreFloatArray256(this.y, i, Avx.Multiply(Avx.Divide(ay, len), result));
                SIMDHelper.StoreFloatArray256(this.z, i, Avx.Multiply(Avx.Divide(az, len), result));

            }
        }
    }

    public struct Entities
    {
        public string[] name;
        public Vectors3 pos;
        public Vectors3 v;
        public float[] mass;
        public float[] elasticity;
        public float[] strength;

        public Entities(int count)
        {
            name = new string[count];
            pos = new Vectors3(count);
            v = new Vectors3(count);
            mass = new float[count];
            elasticity = new float[count];
            strength = new float[count];
        }
    }


}


