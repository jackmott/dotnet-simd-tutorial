
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
           //todo
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

            //todo

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
            //todo
        }

        public void AvxClamp(float min)
        {
            // todo!
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


