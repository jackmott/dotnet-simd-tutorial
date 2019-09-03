using System;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics;

namespace SIMDTutorial
{
    public struct Entity
    {
        public string name;
        public Vector3 pos;
        public Vector3 v;
        public float mass;
        public float elasticity;
        public float strength;

        public Entity(string name, Vector3 pos, Vector3 v, float mass, float elasticity, float strength)
        {
            this.name = name;
            this.pos = pos;
            this.v = v;
            this.mass = mass;
            this.elasticity = elasticity;
            this.strength = strength;
        }
    }

    public struct Vector3
    {
        public float x;
        public float y;
        public float z;

        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public void Add(Vector3 v)
        {
            this.x += v.x;
            this.y += v.y;
            this.z += v.z;
        }

        public void Norm()
        {
            var len = MathF.Sqrt(this.x * this.x + this.y * this.y + this.z * this.z);
            this.x /= len;
            this.y /= len;
            this.z /= len;
        }

        public void SseAdd(Vector3 v)
        {
            var a = Vector128.Create(this.x, this.y, this.z, 0.0f);
            var b = Vector128.Create(v.x, v.y, v.z, 0.0f);

            var result = Sse.Add(a, b);
                       
            
            this.x = result.GetElement(0); //This would be reversed if addressing memory directly
            this.y = result.GetElement(1);
            this.z = result.GetElement(2);
        }

        public void SSE_Norm()
        {
            //no good way to do this!

        }
    }

 
}
