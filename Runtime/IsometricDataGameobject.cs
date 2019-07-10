using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace HMH.ECS.IsoSorting
{
    public class IsometricDataGameobject : MonoBehaviour
    {
        #region Variables

        [SerializeField]
        private Vector3 _position;
        [SerializeField]
        private Vector3 _size;
        [SerializeField]
        private SpriteRenderer _spriteRenderer;

        [SerializeField]
        private Vector2 _screenMin;
        [SerializeField]
        private Vector2 _screenMax;

        private Vector3 _min;
        private Vector3 _max;

        [SerializeField]
        private Entity _entity;
        [SerializeField]
        private int _spatialHashingIndex;
        [SerializeField]
        private int _depth;

        #endregion

        #region Properties

        public Entity Entity => _entity;
        public Vector3 Position { get => _position; set => _position = value; }
        public Vector3 Size { get => _size; set => _size = value; }

        #endregion

        private void Start()
        {
            var dstManager = World.Active.EntityManager;

            _entity = dstManager.CreateEntity(typeof(IsometricData), typeof(IsometricDataNeedUpdate), typeof(IsometricDepthData));

            dstManager.SetComponentData(_entity, new IsometricData { IsoPosition = _position, IsoSize = _size, Entity = _entity });

            World.Active.GetExistingSystem<IsometricSyncSystem>().AddIsometricDataOop(this);
        }

        public void InitForConversion(Vector3 isoPosition, Vector3 isoSize, SpriteRenderer spriteRenderer)
        {
            _position       = isoPosition;
            _size           = isoSize;
            _spriteRenderer = spriteRenderer;
        }

        public void UpdateWorldDataToLocalData(IsometricMatrix matrix, ref IsometricData data, ref IsometricDepthData depthData)
        {
            _spatialHashingIndex = data.SpatialHashingIndex;

            _position = data.IsoPosition;
            _size     = data.IsoSize;

#if SORTBYDEPTH
            transform.position = matrix.IsoToScreen(data.IsoPosition).ToVector3(0.1F * depthData.Depth);
#else
            transform.position = matrix.IsoToScreen(data.IsoPosition).ToVector3();
#endif

            _screenMin = data.ScreenMin;
            _screenMax = data.ScreenMax;

            _min = data.IsoMin;
            _max = data.IsoMax;

#if !SORTBYDEPTH
               if (_spriteRenderer != null)
                   _spriteRenderer.sortingOrder = -depthData.Depth;//sortingorder is inverse of depth
#endif
        }

        public void UpdateLocalDataToWorldData()
        {
            World.Active?.GetExistingSystem<IsometricSyncSystem>()
                 .UpdateIsometricData(this, new IsometricData { IsoPosition = _position, IsoSize = _size, Entity = _entity, SpatialHashingIndexInternal = _spatialHashingIndex });
        }

        private void OnDestroy()
        {
            World.Active?.GetExistingSystem<IsometricSyncSystem>()?.RemoveIsometricDataOop(this, true);
        }

        private void OnDrawGizmosSelected()
        {
            var syncSystem = World.Active?.GetExistingSystem<IsometricSyncSystem>();

            if (syncSystem == null)
                return;

            var matrix = World.Active.GetExistingSystem<IsometricSyncSystem>().GetSingleton<IsometricMatrix>();

            Gizmos.color = Color.green;
            var center = new float3(_screenMin + (_screenMax - _screenMin) * 0.5F, 0F);
            Gizmos.DrawWireCube(center, new float3(_screenMax - _screenMin, 0F));

            Gizmos.color = Color.red;
            DrawWireCube(matrix);
        }

        private void DrawWireCube(IsometricMatrix matrix)
        {
            var bottomRightPosition = new float3(matrix.IsoToScreen(_position), 0F);
            var bottomLeftPosition  = new float3(matrix.IsoToScreen(_position + new Vector3(0F, _size.y, 0F)), 0F);
            var topRightPosition    = new float3(matrix.IsoToScreen(_position + new Vector3(0F, 0F, _size.z)), 0F);
            var topLeftPosition     = new float3(matrix.IsoToScreen(_position + new Vector3(0F, _size.y, _size.z)), 0F);

            var rearBottomRightPosition = new float3(matrix.IsoToScreen(_position + new Vector3(-_size.x, 0F, 0F)), 0F);
            var rearBottomLeftPosition  = new float3(matrix.IsoToScreen(_position + new Vector3(-_size.x, _size.y, 0F)), 0F);
            var rearTopRightPosition    = new float3(matrix.IsoToScreen(_position + new Vector3(-_size.x, 0F, _size.z)), 0F);
            var rearTopLeftPosition     = new float3(matrix.IsoToScreen(_position + new Vector3(-_size.x, _size.y, _size.z)), 0F);


            Gizmos.DrawLine(bottomRightPosition, bottomLeftPosition);
            Gizmos.DrawLine(bottomLeftPosition, topLeftPosition);
            Gizmos.DrawLine(topLeftPosition, topRightPosition);
            Gizmos.DrawLine(topRightPosition, bottomRightPosition);

            Gizmos.DrawLine(rearBottomRightPosition, rearBottomLeftPosition);
            Gizmos.DrawLine(rearBottomLeftPosition, rearTopLeftPosition);
            Gizmos.DrawLine(rearTopLeftPosition, rearTopRightPosition);
            Gizmos.DrawLine(rearTopRightPosition, rearBottomRightPosition);

            Gizmos.DrawLine(bottomRightPosition, rearBottomRightPosition);
            Gizmos.DrawLine(bottomLeftPosition, rearBottomLeftPosition);
            Gizmos.DrawLine(topRightPosition, rearTopRightPosition);
            Gizmos.DrawLine(topLeftPosition, rearTopLeftPosition);
        }
    }
}