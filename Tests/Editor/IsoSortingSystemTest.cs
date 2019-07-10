using HMH.ECS.IsoSorting;
using HMH.ECS.SpatialHashing;
using NUnit.Framework;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Bounds = HMH.ECS.SpatialHashing.Bounds;
using Random = Unity.Mathematics.Random;

namespace Tests
{
    public class IsoSortingSystemTest : ECSTestsFixture
    {
        [Test]
        public void IsIsoObjectOverlapTest()
        {
            IsometricData reference = new IsometricData { ScreenMin = float2.zero, ScreenMax = new float2(10F) };

            IsometricData outsideTopLeft     = new IsometricData { ScreenMin = new float2(-10F, 11), ScreenMax   = new float2(-1, 20F) };
            IsometricData outsideTopRight    = new IsometricData { ScreenMin = new float2(11F, 11F), ScreenMax   = new float2(20F, 20F) };
            IsometricData outsideBottomLeft  = new IsometricData { ScreenMin = new float2(-10F, -10F), ScreenMax = new float2(-1F, -1F) };
            IsometricData outsideBottomRight = new IsometricData { ScreenMin = new float2(11F, -10F), ScreenMax  = new float2(20F, -1F) };

            Assert.IsFalse(IsoSortingSystem.CreationTopologicalListJob.IsIsoObjectOverlap(reference, outsideTopLeft));
            Assert.IsFalse(IsoSortingSystem.CreationTopologicalListJob.IsIsoObjectOverlap(reference, outsideTopRight));
            Assert.IsFalse(IsoSortingSystem.CreationTopologicalListJob.IsIsoObjectOverlap(reference, outsideBottomLeft));
            Assert.IsFalse(IsoSortingSystem.CreationTopologicalListJob.IsIsoObjectOverlap(reference, outsideBottomRight));

            IsometricData insideTopLeft     = new IsometricData { ScreenMin = new float2(-10F, 9), ScreenMax   = new float2(1, 20F) };
            IsometricData insideTopRight    = new IsometricData { ScreenMin = new float2(9.9F, 9.9F), ScreenMax    = new float2(20F, 20F) };
            IsometricData insideBottomLeft  = new IsometricData { ScreenMin = new float2(-10F, -10F), ScreenMax = new float2(0.1F, 1F) };
            IsometricData insideBottomRight = new IsometricData { ScreenMin = new float2(9F, -10F), ScreenMax  = new float2(20F, 0.1F) };

            Assert.IsTrue(IsoSortingSystem.CreationTopologicalListJob.IsIsoObjectOverlap(reference, insideTopLeft));
            Assert.IsTrue(IsoSortingSystem.CreationTopologicalListJob.IsIsoObjectOverlap(reference, insideTopRight));
            Assert.IsTrue(IsoSortingSystem.CreationTopologicalListJob.IsIsoObjectOverlap(reference, insideBottomLeft));
            Assert.IsTrue(IsoSortingSystem.CreationTopologicalListJob.IsIsoObjectOverlap(reference, insideBottomRight));

        }

        [Test]
        public void CreationTopologicalListJobTest()
        {
            var b           = new Bounds(new float3(9400F, 400F, 0.5F), new float3(9600F, 5200F, 0.5F) * 2F);
            var spatialHash = new SpatialHash<IsometricData>(b, new float3(192F, 144F, 1F), Allocator.TempJob);

            var isoObject = new IsometricData
            {
                IsoPosition = new float3(17F, -1F, 0F), IsoSize = new float3(1F, 1F, 1F), ScreenMin = new float2(39.99997F, -144F), ScreenMax = new float2(71.99997F, -112F)
            }; //1
            spatialHash.Add(ref isoObject);

            isoObject = new IsometricData
            {
                IsoPosition = new float3(16F, -1F, 0F), IsoSize = new float3(3F, 1F, 1F), ScreenMin = new float2(-8.000031F, -136F), ScreenMax = new float2(55.99997F, -88F)
            }; //2
            spatialHash.Add(ref isoObject);

            isoObject = new IsometricData
            {
                IsoPosition = new float3(13F, -1F, 3F), IsoSize = new float3(1F, 1F, 1F), ScreenMin = new float2(-24.00002F, -64F), ScreenMax = new float2(7.999985F, -32F)
            }; //3
            spatialHash.Add(ref isoObject);

            isoObject = new IsometricData
            {
                IsoPosition = new float3(17F, -2F, 0F), IsoSize = new float3(3F, 1F, 1F), ScreenMin = new float2(-8.000031F, -152F), ScreenMax = new float2(55.99997F, -104F)
            }; //4
            spatialHash.Add(ref isoObject);

            isoObject = new IsometricData
            {
                IsoPosition = new float3(13F, -4F, 2F), IsoSize = new float3(1F, 3F, 1F), ScreenMin = new float2(-72.00002F, -104F), ScreenMax = new float2(-8.000015F, -56F)
            }; //5
            spatialHash.Add(ref isoObject);

            isoObject = new IsometricData
            {
                IsoPosition = new float3(14F, -2F, 0F), IsoSize = new float3(1F, 1F, 1F), ScreenMin = new float2(-24.00003F, -128F), ScreenMax = new float2(7.999969F, -96F)
            }; //6
            spatialHash.Add(ref isoObject);

            isoObject = new IsometricData
            {
                IsoPosition = new float3(14F, -5F, 0F), IsoSize = new float3(1F, 3F, 1F), ScreenMin = new float2(-72.00003F, -152F), ScreenMax = new float2(-8.000031F, -104F)
            }; //7
            spatialHash.Add(ref isoObject);

            isoObject = new IsometricData
            {
                IsoPosition = new float3(20F, -1F, 0F), IsoSize = new float3(3F, 1F, 1F), ScreenMin = new float2(55.99997F, -168F), ScreenMax = new float2(120F, -120F)
            }; //8
            spatialHash.Add(ref isoObject);

            isoObject = new IsometricData
            {
                IsoPosition = new float3(14F, -1F, 1F), IsoSize = new float3(1F, 1F, 1F), ScreenMin = new float2(-8.000031F, -104F), ScreenMax = new float2(23.99997F, -72F)
            }; //9
            spatialHash.Add(ref isoObject);

            isoObject = new IsometricData
            {
                IsoPosition = new float3(13F, -2F, 1F), IsoSize = new float3(1F, 1F, 1F), ScreenMin = new float2(-40.00002F, -104F), ScreenMax = new float2(-8.000031F, -72F)
            }; //10
            spatialHash.Add(ref isoObject);

            isoObject = new IsometricData
            {
                IsoPosition = new float3(17F, -1F, 1F), IsoSize = new float3(3F, 1F, 1F), ScreenMin = new float2(7.999969F, -128F), ScreenMax = new float2(71.99997F, -80F)
            }; //11
            spatialHash.Add(ref isoObject);

            isoObject = new IsometricData
            {
                IsoPosition = new float3(13F, -1F, 1F), IsoSize = new float3(1F, 1F, 1F), ScreenMin = new float2(-24.00002F, -96F), ScreenMax = new float2(7.999969F, -64F)
            }; //12
            spatialHash.Add(ref isoObject);

            isoObject = new IsometricData
            {
                IsoPosition = new float3(14F, -2F, 1F), IsoSize = new float3(1F, 1F, 1F), ScreenMin = new float2(-24.00003F, -112F), ScreenMax = new float2(7.999969F, -80F)
            }; //13
            spatialHash.Add(ref isoObject);

            isoObject = new IsometricData
            {
                IsoPosition = new float3(18F, -1F, 1F), IsoSize = new float3(1F, 1F, 1F), ScreenMin = new float2(55.99997F, -136F), ScreenMax = new float2(87.99997F, -104F)
            }; //14
            spatialHash.Add(ref isoObject);

            isoObject = new IsometricData
            {
                IsoPosition = new float3(16F, -1F, 2F), IsoSize = new float3(3F, 1F, 1F), ScreenMin = new float2(-8.000015F, -104F), ScreenMax = new float2(55.99997F, -56F)
            }; //15
            spatialHash.Add(ref isoObject);

            isoObject = new IsometricData
            {
                IsoPosition = new float3(13F, -1F, 0F), IsoSize = new float3(1F, 1F, 1F), ScreenMin = new float2(-24.00003F, -112F), ScreenMax = new float2(7.999969F, -80F)
            }; //16
            spatialHash.Add(ref isoObject);

            isoObject = new IsometricData
            {
                IsoPosition = new float3(13F, -5F, 0F), IsoSize = new float3(1F, 1F, 1F), ScreenMin = new float2(-88.00003F, -144F), ScreenMax = new float2(-56.00003F, -112F)
            }; //17
            spatialHash.Add(ref isoObject);

            isoObject = new IsometricData
            {
                IsoPosition = new float3(13F, -1F, 2F), IsoSize = new float3(1F, 1F, 1F), ScreenMin = new float2(-24.00002F, -80F), ScreenMax = new float2(7.999985F, -48F)
            }; //18
            spatialHash.Add(ref isoObject);

            isoObject = new IsometricData
            {
                IsoPosition = new float3(13F, -4F, 0F), IsoSize = new float3(1F, 3F, 1F), ScreenMin = new float2(-72.00003F, -136F), ScreenMax = new float2(-8.000031F, -88F)
            }; //19
            spatialHash.Add(ref isoObject);

            const int iTemCount = 19;

            var chunkIndicies = new NativeList<int3>(Allocator.TempJob);
            var cellCount     = spatialHash.CellCount;

            for (int x = 0; x < cellCount.x; x++)
                for (int y = 0; y < cellCount.y; y++)
                    for (int z = 0; z < cellCount.x; z++)
                        chunkIndicies.Add(new int3(x, y, z));

            var topologicalListFrontToBack = new NativeMultiHashMap<int, int>(60, Allocator.TempJob);
            var queue                      = new NativeQueue<int>(Allocator.TempJob);

            var job = new IsoSortingSystem.CreationTopologicalListJob
            {
                SpatialHashing             = spatialHash,
                ChunkList                  = chunkIndicies,
                TopologicalListFrontToBack = topologicalListFrontToBack.ToConcurrent(),
                IsometricDataPresent       = queue.ToConcurrent()
            };

            job.Schedule(chunkIndicies, 1).Complete();

            var hashset = new HashSet<int>();

            while (queue.Count > 0)
                hashset.Add(queue.Dequeue());

            Assert.AreEqual(iTemCount, hashset.Count);

            FindAllItem(topologicalListFrontToBack, 18, 3, 5, 3, 15);
            FindAllItem(topologicalListFrontToBack, 16, 5, 6, 7, 10, 12, 13, 2, 4, 6, 9, 12, 13, 15, 19);
            FindAllItem(topologicalListFrontToBack, 15);
            FindAllItem(topologicalListFrontToBack, 14);
            FindAllItem(topologicalListFrontToBack, 13);
            FindAllItem(topologicalListFrontToBack, 12, 9, 5, 13, 10, 13, 15, 18, 18);
            FindAllItem(topologicalListFrontToBack, 11, 14, 15);
            FindAllItem(topologicalListFrontToBack, 9, 11, 13, 15);
            FindAllItem(topologicalListFrontToBack, 8, 14);
            FindAllItem(topologicalListFrontToBack, 6, 4, 7, 13, 13);
            FindAllItem(topologicalListFrontToBack, 4);
            FindAllItem(topologicalListFrontToBack, 3);
            FindAllItem(topologicalListFrontToBack, 2, 1, 4, 6, 9, 11, 13, 15);
            FindAllItem(topologicalListFrontToBack, 1, 4, 8, 11, 14);
            FindAllItem(topologicalListFrontToBack, 19, 5, 6, 7, 10, 13, 17);
            FindAllItem(topologicalListFrontToBack, 17, 7);
            FindAllItem(topologicalListFrontToBack, 10, 5, 13);
            FindAllItem(topologicalListFrontToBack, 7);
            FindAllItem(topologicalListFrontToBack, 5);

            spatialHash.Dispose();
            chunkIndicies.Dispose();
            topologicalListFrontToBack.Dispose();
            queue.Dispose();
        }

        private void FindAllItem(NativeMultiHashMap<int, int> collection, int key, params int[] values)
        {
            foreach (var target in values)
            {
                bool found = false;

                if (collection.TryGetFirstValue(key, out int item, out var it) == false)
                    Assert.Fail("Can't find any value for " + key);

                do
                {
                    if (target == item)
                    {
                        found = true;
                        break;
                    }
                } while (collection.TryGetNextValue(out item, ref it));


                if (found == false)
                    Assert.Fail($"Can't find value {target} for key {key}");
            }

            int count = 0;

            if (collection.TryGetFirstValue(key, out int item2, out var it2))
                do
                    ++count;
                while (collection.TryGetNextValue(out item2, ref it2));

            Assert.AreEqual(values.Length, count);
        }

        [Test]
        public void AssignIsometricDepthJobTest()
        {
            var spatialHash = new SpatialHash<IsometricData>(new Bounds(new float3(), new float3(200F)), new float3(100), Allocator.TempJob);

            var topologicalListFrontToBack = new NativeMultiHashMap<int, int>(10, Allocator.TempJob);
            var dataForComputation         = new NativeQueue<int>(Allocator.TempJob);

            #region Creation iso object

            topologicalListFrontToBack.Add(1, 2);
            topologicalListFrontToBack.Add(1, 3);
            topologicalListFrontToBack.Add(2, 4);
            topologicalListFrontToBack.Add(4, 5);
            topologicalListFrontToBack.Add(3, 5);
            topologicalListFrontToBack.Add(6, 5);
            topologicalListFrontToBack.Add(5, 7);

            var r = new Random(456);

            for (int i = 0; i < 50; i++)
                dataForComputation.Enqueue(r.NextInt(1, 8));

            #endregion

            var list1   = new NativeList<int>(Allocator.TempJob);
            var result  = new NativeList<int>(Allocator.TempJob);
            var unicity = new NativeHashMap<int, byte>(10, Allocator.TempJob);

            new IsoSortingSystem.TopologicalSortingJob
            {
                TopologicalListFrontToBack   = topologicalListFrontToBack,
                IsometricDataFromComputation = dataForComputation,
                CurrentLevelList             = list1,
                IsoToDepth                   = unicity,
                IsometricIndexForThisFrame   = result
            }.Run();

            var detected = unicity.GetKeyArray(Allocator.Temp);

            Assert.AreEqual(7, detected.Length);

            for (int i = 1; i < 7 + 1; i++)
            {
                Assert.IsTrue(detected.Contains(i));
                Assert.IsTrue(result.Contains(i));
            }

            Assert.AreEqual(7, result[0]);
            Assert.AreEqual(5, result[1]);
            Assert.AreEqual(3, result[2]);
            Assert.AreEqual(4, result[3]);
            Assert.AreEqual(2, result[4]);
            Assert.AreEqual(1, result[5]);
            Assert.AreEqual(6, result[6]);

            spatialHash.Dispose();
            topologicalListFrontToBack.Dispose();
            dataForComputation.Dispose();
            list1.Dispose();
            unicity.Dispose();
            result.Dispose();
        }
    }
}