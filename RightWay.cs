
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics;


namespace SIMDTutorial
{

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
                    axPtr = &this.x[0],
                    ayPtr = &this.y[0],
                    azPtr = &this.z[0],
                    bxPtr = &v.x[0],
                    byPtr = &v.y[0],
                    bzPtr = &v.z[0])
                {
                    for (int i = 0; i < this.x.Length; i = i + 4)
                    {
                        var ax = Sse.LoadVector128(&axPtr[i]);
                        var ay = Sse.LoadVector128(&ayPtr[i]);
                        var az = Sse.LoadVector128(&azPtr[i]);

                        var bx = Sse.LoadVector128(&bxPtr[i]);
                        var by = Sse.LoadVector128(&byPtr[i]);
                        var bz = Sse.LoadVector128(&bzPtr[i]);

                        Sse.Store(&axPtr[i], Sse.Add(ax, bx));
                        Sse.Store(&ayPtr[i], Sse.Add(ay, by));
                        Sse.Store(&azPtr[i], Sse.Add(az, bz));

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

            unsafe
            {
                fixed (float*
                    axPtr = &this.x[0],
                    ayPtr = &this.y[0],
                    azPtr = &this.z[0])
                {
                    for (int i = 0; i < this.x.Length; i = i + 4)
                    {
                        var ax = Sse.LoadVector128(&axPtr[i]);
                        var ay = Sse.LoadVector128(&ayPtr[i]);
                        var az = Sse.LoadVector128(&azPtr[i]);

                        // len = 1/sqrt(x*x+y*y+z*z)
                        var len = Sse.ReciprocalSqrt(Sse.Add(Sse.Add(Sse.Multiply(ax, ax), Sse.Multiply(ay, ay)), Sse.Multiply(az, az)));

                        Sse.Store(&axPtr[i], Sse.Multiply(ax, len));
                        Sse.Store(&ayPtr[i], Sse.Multiply(ay, len));
                        Sse.Store(&azPtr[i], Sse.Multiply(az, len));
                    }
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

            unsafe
            {
                fixed (float*
                    axPtr = &this.x[0],
                    ayPtr = &this.y[0],
                    azPtr = &this.z[0])
                {
                    for (int i = 0; i < this.x.Length; i = i + 4)
                    {
                        var ax = Sse.LoadVector128(&axPtr[i]);
                        var ay = Sse.LoadVector128(&ayPtr[i]);
                        var az = Sse.LoadVector128(&azPtr[i]);


                        // len = sqrt(x*x+y*y+z*z)
                        var len = Sse.Sqrt(Sse.Add(Sse.Add(Sse.Multiply(ax, ax), Sse.Multiply(ay, ay)), Sse.Multiply(az, az)));
                        var mask = Sse.CompareLessThan(len, trueResult);
                        var result = Sse.Or(Sse.And(mask, trueResult), Sse.AndNot(mask, falseResult));

                        Sse.Store(&axPtr[i], Sse.Multiply(Sse.Divide(ax, len), result));
                        Sse.Store(&ayPtr[i], Sse.Multiply(Sse.Divide(ay, len), result));
                        Sse.Store(&azPtr[i], Sse.Multiply(Sse.Divide(az, len), result));
                    }

                }
            }
        }

        public void AvxClamp(float min)
        {
            var trueResult = Vector256.Create(min);
            var falseResult = Vector256.Create(1.0f);

            unsafe
            {
                fixed (float*
                    axPtr = &this.x[0],
                    ayPtr = &this.y[0],
                    azPtr = &this.z[0])
                {
                    for (int i = 0; i < this.x.Length; i = i + 8)
                    {
                        var ax = Avx.LoadVector256(&axPtr[i]);
                        var ay = Avx.LoadVector256(&ayPtr[i]);
                        var az = Avx.LoadVector256(&azPtr[i]);

                        // len = sqrt(x*x+y*y+z*z)
                        var len = Avx.Sqrt(Avx.Add(Avx.Add(Avx.Multiply(ax, ax), Avx.Multiply(ay, ay)), Avx.Multiply(az, az)));
                        Vector256<float> mask = Avx.Compare(len, trueResult, FloatComparisonMode.OrderedLessThanSignaling);
                        var result = Avx.BlendVariable(falseResult, trueResult, mask);

                        Avx.Store(&axPtr[i], Avx.Multiply(Avx.Divide(ax, len), result));
                        Avx.Store(&ayPtr[i], Avx.Multiply(Avx.Divide(ay, len), result));
                        Avx.Store(&azPtr[i], Avx.Multiply(Avx.Divide(az, len), result));

                    }
                }
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


