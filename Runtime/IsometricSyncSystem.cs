using System.Collections.Generic;
using Unity.Entities;

namespace HMH.ECS.IsoSorting
{
#if UNITY_EDITOR
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class IsometricSyncSystem : ComponentSystem
    {
        /// <inheritdoc />
        protected override void OnCreate()
        {
            _updateGroup = GetEntityQuery(ComponentType.ReadWrite<IsometricData>(), ComponentType.ReadOnly<IsometricDataMirror>(), ComponentType.ReadOnly<IsometricDataNeedUpdate>());
        }

        /// <inheritdoc />
        protected override void OnUpdate()
        {
            if (HasSingleton<IsometricMatrix>() == false)
                return;

            var matrix = GetSingleton<IsometricMatrix>();

            Entities.WithAll<IsometricData, IsometricDepthData, IsometricDataNeedUpdate>()
                .ForEach((ref IsometricData isoData, ref IsometricDepthData isoDepth) =>
                {
                    if (_linkEntity2GameObject.TryGetValue(isoData.Entity, out var isoOOP))
                        isoOOP.UpdateWorldDataToLocalData(matrix, ref isoData, ref isoDepth);
                });

            PostUpdateCommands.RemoveComponent(_updateGroup, typeof(IsometricDataNeedUpdate));

            foreach (var data in _isometricDataToUpdate)
            {
                PostUpdateCommands.AddComponent(data.Key.Entity, new IsometricDataNeedUpdate());
                EntityManager.SetComponentData(data.Key.Entity, data.Value);
            }

            _isometricDataToUpdate.Clear();

            foreach (var data in _isoObjectToRemove)
                EntityManager.DestroyEntity(data.Entity);

            _isoObjectToRemove.Clear();
        }

        public void AddIsometricDataOop(IsometricDataGameobject data)
        {
            _linkEntity2GameObject[data.Entity] = data;
        }

        public void RemoveIsometricDataOop(IsometricDataGameobject data, bool removeEntity)
        {
            _linkEntity2GameObject.Remove(data.Entity);

            if (removeEntity)
                _isoObjectToRemove.Add(data);
        }

        public void UpdateIsometricData(IsometricDataGameobject isoGO, IsometricData newData)
        {
            _isometricDataToUpdate[isoGO] = newData;
        }

        #region Variables

        private EntityQuery _updateGroup;

        private Dictionary<Entity, IsometricDataGameobject> _linkEntity2GameObject = new Dictionary<Entity, IsometricDataGameobject>(1024);
        private Dictionary<IsometricDataGameobject, IsometricData> _isometricDataToUpdate = new Dictionary<IsometricDataGameobject, IsometricData>();
        private HashSet<IsometricDataGameobject> _isoObjectToRemove = new HashSet<IsometricDataGameobject>();

        #endregion
    }
#endif
}