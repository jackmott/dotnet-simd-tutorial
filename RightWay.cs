
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

            unsafe
            {
                fixed (float*
                    ax = &this.x[0],
                    ay = &this.y[0],
                    az = &this.z[0])
                {
                    for (int i = 0; i < this.x.Length; i = i + 4)
                    {
                        var axV = Sse.LoadVector128(&ax[i]);
                        var ayV = Sse.LoadVector128(&ay[i]);
                        var azV = Sse.LoadVector128(&az[i]);

                        // len = 1/sqrt(x*x+y*y+z*z)
                        var len = Sse.ReciprocalSqrt(Sse.Add(Sse.Add(Sse.Multiply(axV, axV), Sse.Multiply(ayV, ayV)), Sse.Multiply(azV, azV)));

                        Sse.Store(&ax[i], Sse.Multiply(axV, len));
                        Sse.Store(&ay[i], Sse.Multiply(ayV, len));
                        Sse.Store(&az[i], Sse.Multiply(azV, len));
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
                    ax = &this.x[0],
                    ay = &this.y[0],
                    az = &this.z[0])
                {
                    for (int i = 0; i < this.x.Length; i = i + 4)
                    {
                        var axV = Sse.LoadVector128(&ax[i]);
                        var ayV = Sse.LoadVector128(&ay[i]);
                        var azV = Sse.LoadVector128(&az[i]);


                        // len = sqrt(x*x+y*y+z*z)
                        var len = Sse.Sqrt(Sse.Add(Sse.Add(Sse.Multiply(axV, axV), Sse.Multiply(ayV, ayV)), Sse.Multiply(azV, azV)));
                        var mask = Sse.CompareLessThan(len, trueResult);
                        var result = Sse.Or(Sse.And(mask, trueResult), Sse.AndNot(mask, falseResult));

                        Sse.Store(&ax[i], Sse.Multiply(Sse.Divide(axV, len), result));
                        Sse.Store(&ay[i], Sse.Multiply(Sse.Divide(ayV, len), result));
                        Sse.Store(&az[i], Sse.Multiply(Sse.Divide(azV, len), result));
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
                    ax = &this.x[0],
                    ay = &this.y[0],
                    az = &this.z[0])
                {
                    for (int i = 0; i < this.x.Length; i = i + 8)
                    {
                        var axV = Avx.LoadVector256(&ax[i]);
                        var ayV = Avx.LoadVector256(&ay[i]);
                        var azV = Avx.LoadVector256(&az[i]);

                        // len = sqrt(x*x+y*y+z*z)
                        var len = Avx.Sqrt(Avx.Add(Avx.Add(Avx.Multiply(axV, axV), Avx.Multiply(ayV, ayV)), Avx.Multiply(azV, azV)));
                        Vector256<float> mask = Avx.Compare(len, trueResult, FloatComparisonMode.OrderedLessThanSignaling);
                        var result = Avx.BlendVariable(falseResult, trueResult, mask);

                        Avx.Store(&ax[i], Avx.Multiply(Avx.Divide(axV, len), result));
                        Avx.Store(&ay[i], Avx.Multiply(Avx.Divide(ayV, len), result));
                        Avx.Store(&az[i], Avx.Multiply(Avx.Divide(azV, len), result));

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


