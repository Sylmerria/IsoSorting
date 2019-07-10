using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

#if UNITY_EDITOR
namespace HMH.ECS.IsoSorting
{
    public class IsometricHashingSytemGizmo : MonoBehaviour
    {
        private void OnEnable()
        {
            var spatialHashing = World.Active.GetOrCreateSystem<IsometricHashingSystem>().SpatialHashing;
            _cellCount = spatialHashing.CellCount;
            _cellSize  = spatialHashing.CellSize;
            _minBound  = spatialHashing.WorldBounds.Min;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;

            for (int x = 0; x < _cellCount.x; x++)
            {
                for (int y = 0; y < _cellCount.y; y++)
                {
                    for (int z = 0; z < _cellCount.z; z++)
                    {
                        var center = new Vector3((x + 0.5F) * _cellSize.x, (y + 0.5F) * _cellSize.y, (z + 0.5F) * _cellSize.z) + _minBound;
                        Gizmos.DrawWireCube(center, _cellSize);
                    }
                }
            }

            Gizmos.color = Color.white;
        }

        #region Variables

        private float3  _cellSize;
        private int3    _cellCount;
        private Vector3 _minBound;

        #endregion
    }
}
#endif