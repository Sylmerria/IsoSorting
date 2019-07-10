using HMH.ECS.SpatialHashing;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using Bounds = HMH.ECS.SpatialHashing.Bounds;

namespace HMH.ECS.IsoSorting
{
    public class IsoSortingSystem : JobComponentSystem
    {
        #region Overrides of JobComponentSystem

        /// <inheritdoc />
        protected override void OnCreate()
        {
            _spatialHashing = World.GetOrCreateSystem<IsometricHashingSystem>().SpatialHashing;

            _cameraDataList = new NativeList<QuerySpatialHashingJob.CameraData>(4, Allocator.Persistent);

            _chunckVisible              = new NativeList<int3>(16, Allocator.Persistent);
            _chunckVisibleHelpIteration = new NativeList<int3>(16, Allocator.Persistent);
            _chunckVisibleHelpUnicity   = new NativeHashMap<int3, byte>(16, Allocator.Persistent);

            _topologicalListFrontToBack          = new NativeMultiHashMap<int, int>(1 << 19, Allocator.Persistent);
            _isometricDataPresentOnCameraView    = new NativeQueue<int>(Allocator.Persistent);
            _isometricDepthAssigned              = new NativeHashMap<int, byte>(1 << 16, Allocator.Persistent);
            _helpTopologicalList                 = new NativeList<int>(1 << 16, Allocator.Persistent);
            _isometricElementFromTopologicalList = new NativeList<int>(1 << 19, Allocator.Persistent);
        }

        /// <inheritdoc />
        protected override void OnDestroy()
        {
            _chunckVisible.Dispose();
            _chunckVisibleHelpIteration.Dispose();
            _chunckVisibleHelpUnicity.Dispose();
            _topologicalListFrontToBack.Dispose();
            _isometricDataPresentOnCameraView.Dispose();
            _isometricDepthAssigned.Dispose();
            _helpTopologicalList.Dispose();
            _cameraDataList.Dispose();
            _isometricElementFromTopologicalList.Dispose();
        }

        /// <inheritdoc />
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (HasSingleton<IsometricMatrix>() == false)
                return inputDeps;

            inputDeps = JobHandle.CombineDependencies(inputDeps, World.GetOrCreateSystem<IsometricHashingSystem>().JobHandle);
            inputDeps.Complete();
            var cameras = Camera.allCameras;
            _cameraDataList.Clear();

            foreach (var camera in cameras)
                AddCameraToSort(camera);

#if UNITY_EDITOR
            foreach (SceneView sceneView in SceneView.sceneViews)
                AddCameraToSort(sceneView.camera);
#endif

            inputDeps = new QuerySpatialHashingJob
            {
                SpatialHashing      = _spatialHashing,
                CameraDatas         = _cameraDataList,
                ResultHelpIteration = _chunckVisibleHelpIteration,
                ResultHelpUnicity   = _chunckVisibleHelpUnicity,
                Result              = _chunckVisible
            }.Schedule(inputDeps);

            inputDeps = new CreationTopologicalListJob
            {
                SpatialHashing             = _spatialHashing,
                ChunkList                  = _chunckVisible,
                TopologicalListFrontToBack = _topologicalListFrontToBack.ToConcurrent(),
                IsometricDataPresent       = _isometricDataPresentOnCameraView.ToConcurrent()
            }.Schedule(_chunckVisible, 8, inputDeps);

            inputDeps = new TopologicalSortingJob
            {
                TopologicalListFrontToBack   = _topologicalListFrontToBack,
                IsometricDataFromComputation = _isometricDataPresentOnCameraView,
                CurrentLevelList             = _helpTopologicalList,
                IsoToDepth                   = _isometricDepthAssigned,
                IsometricIndexForThisFrame   = _isometricElementFromTopologicalList
            }.Schedule(inputDeps);

            inputDeps = new AssignIsoDepthJob
            {
                SpatialHash = _spatialHashing, IsometricIndexForThisFrame = _isometricElementFromTopologicalList, IsometricDepthCDFE = GetComponentDataFromEntity<IsometricDepthData>()
            }.Schedule(_isometricElementFromTopologicalList, 64, inputDeps);

            inputDeps = new IsoSortingClearCollectionJob
            {
                TopologicalListFrontToBack          = _topologicalListFrontToBack,
                IsometricDataPresentOnCameraView    = _isometricDataPresentOnCameraView,
                HelpTopologicalList                 = _helpTopologicalList,
                IsometricDepthAssigned              = _isometricDepthAssigned,
                IsometricElementFromTopologicalList = _isometricElementFromTopologicalList
            }.Schedule(inputDeps);

            //Add this for be sure end of calculations before spatial hashing update
            World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>().AddJobHandleForProducer(inputDeps);
            return inputDeps;
        }

        private void AddCameraToSort(Camera camera)
        {
            float3 min = camera.ViewportToWorldPoint(new Vector3(0F, 1F));
            float3 max = camera.ViewportToWorldPoint(new Vector3(1F, 0F, camera.farClipPlane));

            var b = new Bounds();
            b.SetMinMax(math.min(min, max), math.max(min, max));

            _cameraDataList.Add(new QuerySpatialHashingJob.CameraData { Bounds = b });
        }

        #endregion

        #region Job

        [BurstCompile]
        public struct QuerySpatialHashingJob : IJob
        {
            #region Implementation of IJob

            /// <inheritdoc />
            public void Execute()
            {
                ResultHelpUnicity.Clear();
                Result.Clear();

                for (int i = 0; i < CameraDatas.Length; i++)
                {
                    ResultHelpIteration.Clear();
                    SpatialHashing.Query(CameraDatas[i].Bounds, ResultHelpIteration);

                    for (int j = 0; j < ResultHelpIteration.Length; j++)
                    {
                        if (ResultHelpUnicity.TryAdd(ResultHelpIteration[j], 0))
                            Result.Add(ResultHelpIteration[j]);
                    }
                }
            }

            #endregion

            #region Variables

            public SpatialHash<IsometricData> SpatialHashing;
            [ReadOnly]
            public NativeList<CameraData> CameraDatas;

            public NativeHashMap<int3, byte> ResultHelpUnicity;
            public NativeList<int3>          ResultHelpIteration;
            public NativeList<int3>          Result;

            #endregion

            public struct CameraData
            {
                public Bounds Bounds;
            }
        }

        [BurstCompile]
        public struct IsoSortingClearCollectionJob : IJob
        {
            #region Variables

            public NativeMultiHashMap<int, int> TopologicalListFrontToBack;
            public NativeQueue<int>             IsometricDataPresentOnCameraView;
            public NativeHashMap<int, byte>     IsometricDepthAssigned;
            public NativeList<int>              IsometricElementFromTopologicalList;
            public NativeList<int>              HelpTopologicalList;

            #endregion

            #region Implementation of IJob

            /// <inheritdoc />
            public void Execute()
            {
                TopologicalListFrontToBack.Clear();
                IsometricDataPresentOnCameraView.Clear();
                IsometricDepthAssigned.Clear();
                IsometricElementFromTopologicalList.Clear();
                HelpTopologicalList.Clear();
            }

            #endregion
        }

        /// <summary>
        /// For each chunk parse N element on N-1 others to find object behind current to a create "topological" list
        /// </summary>
        [BurstCompile]
        public struct CreationTopologicalListJob : IJobParallelForDefer
        {
            #region Implementation of IJobParallelForDefer

            /// <inheritdoc />
            public void Execute(int index)
            {
                var chunkIndex = ChunkList[index];

                int itemCount   = SpatialHashing.QueryCount(chunkIndex);
                var isoDataList = new NativeList<IsometricData>(itemCount, Allocator.Temp);

                SpatialHashing.Query(chunkIndex, isoDataList);

                for (int i = 0; i < itemCount; i++)
                {
                    var currentIsoData = isoDataList[i];

                    Assert.IsTrue(currentIsoData.SpatialHashingIndex > 0);

                    IsometricDataPresent.Enqueue(currentIsoData.SpatialHashingIndex);

                    for (int j = i + 1; j < itemCount; j++)
                    {
                        var comparisonIsoData = isoDataList[j];

                        if (IsIsoObjectOverlap(currentIsoData, comparisonIsoData) && IsObjectIsInFrontOf(currentIsoData, comparisonIsoData))
                            TopologicalListFrontToBack.Add(currentIsoData.SpatialHashingIndex, comparisonIsoData.SpatialHashingIndex);

                        if (IsIsoObjectOverlap(comparisonIsoData, currentIsoData) && IsObjectIsInFrontOf(comparisonIsoData, currentIsoData))
                            TopologicalListFrontToBack.Add(comparisonIsoData.SpatialHashingIndex, currentIsoData.SpatialHashingIndex);
                    }
                }
            }

            public static bool IsIsoObjectOverlap(IsometricData isoObjectLeft, IsometricData isoObjectRight)
            {
                return math.all((isoObjectLeft.ScreenMin < isoObjectRight.ScreenMax) & (isoObjectLeft.ScreenMax > isoObjectRight.ScreenMin));
            }

            public static bool IsObjectIsInFrontOf(IsometricData isoObjectLeft, IsometricData isoObjectRight)
            {
                var leftMinCal  = new float3(-isoObjectLeft.IsoPosition.x, isoObjectLeft.IsoPosition.y, isoObjectRight.IsoPosition.z);
                var leftMaxCal  = leftMinCal + new float3(isoObjectLeft.IsoSize.xy, isoObjectRight.IsoSize.z);
                var rightMinCal = new float3(-isoObjectRight.IsoPosition.x, isoObjectRight.IsoPosition.y, isoObjectLeft.IsoPosition.z);
                var rightMaxCal = rightMinCal + new float3(isoObjectRight.IsoSize.xy, isoObjectLeft.IsoSize.z);

                //from https://shaunlebron.github.io/IsometricBlocks/
                bool leftCanBeInFront = math.all(leftMaxCal > rightMinCal);

                if (leftCanBeInFront)
                {
                    //from http://bannalia.blogspot.com/2008/02/filmation-math.html
                    bool rightCanBeInFront = math.all(rightMaxCal > leftMinCal);

                    if (rightCanBeInFront)
                    {
                        //left and right can be in front, search deeper
                        var deltaLeftToRight = leftMaxCal - rightMinCal;
                        var deltaRightToLeft = rightMaxCal - leftMinCal;

                        var deltaProjection = isoObjectLeft.IsoSize + isoObjectRight.IsoSize - math.abs(deltaRightToLeft - deltaLeftToRight);

                        var condition = new bool2(deltaProjection.y <= deltaProjection.x && deltaProjection.y <= deltaProjection.z,
                                                  deltaProjection.x <= deltaProjection.y && deltaProjection.x <= deltaProjection.z);

                        //better than an if else if for vectorization
                        var res = new bool3(condition.x & (deltaLeftToRight.y > deltaRightToLeft.y),
                                            (condition.x == false) & condition.y & (deltaLeftToRight.x > deltaRightToLeft.x),
                                            (math.any(condition.xy) == false) & (deltaLeftToRight.z > deltaRightToLeft.z));

                        return math.any(res);
                    }
                }

                return leftCanBeInFront;
            }

            #endregion

            #region Variables

            [ReadOnly]
            public SpatialHash<IsometricData> SpatialHashing;
            [ReadOnly]
            public NativeList<int3> ChunkList;

            [WriteOnly]
            public NativeMultiHashMap<int, int>.Concurrent TopologicalListFrontToBack;
            [WriteOnly]
            public NativeQueue<int>.Concurrent IsometricDataPresent;

            #endregion
        }

        //See https://www.youtube.com/watch?v=tFpvX8T0-Pw
        [BurstCompile]
        public struct TopologicalSortingJob : IJob
        {
            #region Implementation of IJob

            /// <inheritdoc />
            public void Execute()
            {
                CurrentLevelList.Clear();
                IsoToDepth.Clear();

                //Parse all object in cameras for find tops
                while (IsometricDataFromComputation.Count > 0)
                {
                    var value = IsometricDataFromComputation.Dequeue();

                    if (IsoToDepth.TryAdd(value, 0))
                        CurrentLevelList.Add(value);
                }

                IsoToDepth.Clear();

                for (int i = 0; i < CurrentLevelList.Length; i++)
                    ParseNode(CurrentLevelList[i]);
            }

            private void ParseNode(int node)
            {
                if (IsoToDepth.TryAdd(node, 0) == false)
                    return;

                //no children nothing to do
                if (TopologicalListFrontToBack.TryGetFirstValue(node, out var childNode, out var it))
                    do
                        ParseNode(childNode);
                    while (TopologicalListFrontToBack.TryGetNextValue(out childNode, ref it));

                IsometricIndexForThisFrame.Add(node);
            }

            #endregion

            #region Variables

            [ReadOnly]
            public NativeMultiHashMap<int, int> TopologicalListFrontToBack;

            public NativeQueue<int> IsometricDataFromComputation;

            public NativeList<int>          IsometricIndexForThisFrame;
            public NativeList<int>          CurrentLevelList;
            public NativeHashMap<int, byte> IsoToDepth;

            #endregion
        }

        [BurstCompile]
        public struct AssignIsoDepthJob : IJobParallelForDefer
        {
            /// <inheritdoc />
            public void Execute(int index)
            {
                var e = SpatialHash.GetObject(IsometricIndexForThisFrame[index]);
                IsometricDepthCDFE[e.Entity] = new IsometricDepthData { Depth = index };
            }

            #region Variables

            [ReadOnly]
            public NativeList<int> IsometricIndexForThisFrame;
            [ReadOnly]
            public SpatialHash<IsometricData> SpatialHash;
            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<IsometricDepthData> IsometricDepthCDFE;

            #endregion
        }

        #endregion

        #region Variables

        private SpatialHash<IsometricData> _spatialHashing;

        private NativeList<QuerySpatialHashingJob.CameraData> _cameraDataList;

        private NativeList<int3>          _chunckVisible;
        private NativeList<int3>          _chunckVisibleHelpIteration;
        private NativeHashMap<int3, byte> _chunckVisibleHelpUnicity;

        private NativeMultiHashMap<int, int> _topologicalListFrontToBack;
        private NativeQueue<int>             _isometricDataPresentOnCameraView;
        private NativeHashMap<int, byte>     _isometricDepthAssigned;
        private NativeList<int>              _isometricElementFromTopologicalList;
        private NativeList<int>              _helpTopologicalList;

        #endregion
    }
}