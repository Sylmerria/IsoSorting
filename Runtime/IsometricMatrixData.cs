using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace HMH.ECS.IsoSorting
{
    [CreateAssetMenu(fileName = "IsometricMatrixData", menuName = "Isometric/IsometricMatrixData", order = 1)]
    public class IsometricMatrixData : ScriptableObject
    {
        #region Variables

        /// <summary>
        /// Offset from world position to isometric position
        /// </summary>
        [SerializeField]
        private float3 _offsetWorldToIsometric;

        [SerializeField]
        private float _tileSize;
        [SerializeField]
        private float _tileRatio;
        [SerializeField]
        private float _tileAngle;
        [SerializeField]
        private float _tileHeight;

        #endregion

        public static IsometricMatrix CreateIsometricMatrix(IsometricMatrixData isoData)
        {
            var isoMatrix = new IsometricMatrix(new IsometricMatrix.IsometricMatrixData
            {
                OffsetWorldToIsometric = isoData._offsetWorldToIsometric,
                TileSize               = isoData._tileSize,
                TileRatio              = isoData._tileRatio,
                TileAngle              = isoData._tileAngle,
                TileHeight             = isoData._tileHeight
            });

            return isoMatrix;
        }

        public static void CreateSingleton(IsometricMatrixData isoData)
        {
            if (World.Active == null)
                World.Active = new World("Editor World");

            var dstManager = World.Active.EntityManager;

            var singletonGroup = dstManager.CreateEntityQuery(typeof(IsometricMatrix));
            var matrix         = CreateIsometricMatrix(isoData);
            dstManager.AddComponentData(dstManager.CreateEntity(), matrix);
            singletonGroup.SetSingleton(matrix);
        }
    }
}