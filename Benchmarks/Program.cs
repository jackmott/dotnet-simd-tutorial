using System;
using SIMDTutorial;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Jobs;

namespace Benchmarks
{
    [MemoryDiagnoser]
    public class Benchmarks
    {
        const int TEST_SIZE = 100000;
        Random rand = new Random(0);
        Entity[] entities;

        public Benchmarks()
        {
        }

        public float RandomFloat() {
            return (float)this.rand.NextDouble();
        }

        [GlobalSetup]
        public void Setup()
        {
            
                
            entities = new Entity[TEST_SIZE];
            for (int i = 0; i < entities.Length; i++)
            {
                var pos = new Vector3(RandomFloat(), RandomFloat(), RandomFloat());
                var v = new Vector3(RandomFloat(), RandomFloat(), RandomFloat());
                entities[i] = new Entity("test",pos, v, 1.0f, 1.0f, 1.0f);
            }

        }


        [Benchmark]
        public void MoveEntities()
        {
            foreach (var e in entities)
            {
                e.pos.Add(e.v);
            }
        }

        
        [Benchmark]
        public void SseMoveEntities()
        {
            foreach (var e in entities)
            {
                e.pos.SseAdd(e.v);
            }
        }

    }


    [MemoryDiagnoser]
    public class BenchmarksSOA
    {
        const int TEST_SIZE = 100000;
        Random rand = new Random(0);
        Entities entities;

        public BenchmarksSOA()
        {
        }

        public float RandomFloat()
        {
            return (float)this.rand.NextDouble();
        }

        [GlobalSetup]
        public void Setup()
        {


            entities = new Entities(TEST_SIZE);
            for (int i = 0; i < TEST_SIZE; i++)
            {
                entities.pos.x[i] = RandomFloat();
                entities.pos.y[i] = RandomFloat();
                entities.pos.z[i] = RandomFloat();

                entities.v.x[i] = RandomFloat();
                entities.v.y[i] = RandomFloat();
                entities.v.z[i] = RandomFloat();

                entities.name[i] = "test";
                entities.elasticity[i] = 1.0f;
                entities.strength[i] = 1.0f;
                entities.mass[i] = 1.0f;

            }

        }

        
        [Benchmark]
        public void MoveEntities()
        {
            entities.pos.Add(entities.v);
        }

        
        [Benchmark]
        public void SseMoveEntities()
        {
            entities.pos.SseAdd(entities.v);
        }

        /*
        [Benchmark]
        public void Norm()
        {
            entities.pos.Norm();
        }

        [Benchmark]
        public void SseNorm()
        {
            entities.pos.SseNorm();
        }
        
        [Benchmark]
        public void Clamp()
        {
            entities.pos.Clamp(0.5f);
        }

        [Benchmark]
        public void SseClamp()
        {
            entities.pos.SseClamp(0.5f);
        }
        

        [Benchmark]
        public void AvxClamp()
        {
            entities.pos.AvxClamp(0.5f);
        }

    */


        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<Benchmarks>(
                ManualConfig.Create(DefaultConfig.Instance).With(Job.ShortRun)
                );
            
        }
    }


}
