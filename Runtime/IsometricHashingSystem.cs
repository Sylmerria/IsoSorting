using HMH.ECS.SpatialHashing;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Bounds = HMH.ECS.SpatialHashing.Bounds;

namespace HMH.ECS.IsoSorting
{
    [UpdateBefore(typeof(IsoSortingSystem))]
    public class IsometricHashingSystem : SpatialHashingSystem<IsometricData, IsometricDataMirror, IsometricDataNeedUpdate>
    {
        /// <inheritdoc />
        protected override void OnCreateManager()
        {
            IsometricMatrixData.CreateSingleton(Resources.Load<IsometricMatrixData>("IsometricMatrixData"));
            RemoveUpdateComponent = false;

            base.OnCreateManager();
            _endSimulationBarrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void AddJobHandleForProducer(JobHandle inputDeps)
        {
            _endSimulationBarrier.AddJobHandleForProducer(inputDeps);
            _jobHandle = inputDeps;
        }

        #region Overrides of SpatialHashingSystem<IsometricData,IsometricDataMirror>

        /// <inheritdoc />
        protected override void InitSpatialHashing()
        {
            var matrix = GetSingleton<IsometricMatrix>();

            var size           = new float3(600, 600, 50);
            var pivot          = new float3(size.x, 0F, 0F);
            var bottomPosition = matrix.IsoToScreen(pivot);
            var topPosition    = matrix.IsoToScreen(new float3(pivot.x - size.x, pivot.yz + size.yz));
            var isoBottom      = pivot;
            isoBottom.y += size.y;
            var rightPosition = matrix.IsoToScreen(isoBottom);
            var leftPosition  = matrix.IsoToScreen(new float3(pivot.x - size.x, pivot.yz));

            var screenMin = new float2(leftPosition.x, bottomPosition.y);
            var screenMax = new float2(rightPosition.x, topPosition.y);

            var bounds = new Bounds();
            bounds.SetMinMax(new float3(screenMin, 0F), new float3(screenMax, 1F));

            size           =  new float3(6, 6, 3);
            pivot          =  new float3(size.x, 0F, 0F);
            bottomPosition =  matrix.IsoToScreen(pivot);
            topPosition    =  matrix.IsoToScreen(new float3(pivot.x - size.x, pivot.yz + size.yz));
            isoBottom      =  pivot;
            isoBottom.y    += size.y;
            rightPosition  =  matrix.IsoToScreen(isoBottom);
            leftPosition   =  matrix.IsoToScreen(new float3(pivot.x - size.x, pivot.yz));

            screenMin = new float2(leftPosition.x, bottomPosition.y);
            screenMax = new float2(rightPosition.x, topPosition.y);

            Debug.Log($"SpatialHashing Bounds {bounds} CellSize {new float3(screenMax - screenMin, 1F)}");
            _spatialHash = new SpatialHash<IsometricData>(bounds, new float3(screenMax - screenMin, 1F), Allocator.Persistent);
        }

        #endregion

        #region Variables

        private EndSimulationEntityCommandBufferSystem _endSimulationBarrier;
        private JobHandle                              _jobHandle;

        #endregion

        #region Properties

        #region Overrides of SpatialHashingSystem<IsometricData,IsometricDataMirror>

        /// <inheritdoc />
        protected override EntityCommandBuffer CommandBuffer => _endSimulationBarrier.CreateCommandBuffer();

        #endregion

        public SpatialHash<IsometricData> SpatialHashing => _spatialHash;
        public JobHandle JobHandle => _jobHandle;

        #endregion
    }
}