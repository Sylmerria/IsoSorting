using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;

namespace HMH.ECS.IsoSorting
{
    [UpdateBefore(typeof(IsometricHashingSystem))]
    public class UpdateIsometricDataScreenSystem : JobComponentSystem
    {
        /// <inheritdoc />
        protected override void OnCreate()
        {
            _isoDataToUpdateQuery = GetEntityQuery(typeof(IsometricData), typeof(IsometricDataNeedUpdate));
        }

        /// <inheritdoc />
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            inputDeps = new UpdateIsometricDataScreenBoundJob { IsometricMatrix = GetSingleton<IsometricMatrix>() }.Schedule(_isoDataToUpdateQuery, inputDeps);

            return inputDeps;
        }

        [BurstCompile, RequireComponentTag(typeof(IsometricDataNeedUpdate))]
        public struct UpdateIsometricDataScreenBoundJob : IJobForEach<IsometricData>
        {
            /// <inheritdoc />
            public void Execute(ref IsometricData data)
            {
                data.UpdateScreenPosition(IsometricMatrix);
            }

            #region Variables

            public IsometricMatrix IsometricMatrix;

            #endregion
        }

        #region Variables

        private EntityQuery                            _isoDataToUpdateQuery;
        private EndSimulationEntityCommandBufferSystem _endSimulationBarrier;

        #endregion
    }
}