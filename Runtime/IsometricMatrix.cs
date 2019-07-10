using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace HMH.ECS.IsoSorting
{
    [System.Serializable]
    public struct IsometricMatrix : IComponentData
    {
        #region Variables

        private float4x4            _isometricMatrix;
        private float4x4            _isometricReverseMatrix;
        private IsometricMatrixData _isometricData;

        #endregion

        public float4x4 Matrix => _isometricMatrix;
        public float4x4 ReverseMatrix => _isometricReverseMatrix;
        public IsometricMatrixData IsometricData => _isometricData;

        public IsometricMatrix(IsometricMatrixData isoData)
        {
            _isometricMatrix = _isometricReverseMatrix = float4x4.identity;
            _isometricData   = isoData;

            SetIsometricData(isoData);
        }

        public void SetIsometricData(IsometricMatrixData isoData)
        {
            _isometricData = isoData;
            CalculIsometricMatrix();
        }

        public void CalculIsometricMatrix()
        {
            _isometricMatrix = math.mul(float4x4.Scale(new float3(1.0f, _isometricData.TileRatio, 1.0f)),
                                        float4x4.TRS(_isometricData.OffsetWorldToIsometric,
                                                     quaternion.AxisAngle(new float3(0F, 0F, -1F), math.radians(90.0f - _isometricData.TileAngle)),
                                                     new float3(_isometricData.TileSize * Mathf.Sqrt(2), _isometricData.TileSize * Mathf.Sqrt(2), _isometricData.TileHeight)));

            _isometricReverseMatrix = math.inverse(_isometricMatrix);
        }

        public float2 IsoToScreen(float3 isoPosition)
        {
            return math.mul(_isometricMatrix, new float4(isoPosition.xy + new float2(-isoPosition.z, isoPosition.z), 0F, 1F)).xy;
        }

        public float3 ScreenToIso(float2 screenPosition, float height = 0F)
        {
            return ScreenToIso(new float3(screenPosition, height));
        }

        public float3 ScreenToIso(float3 screenPosition)
        {
            var pos = math.mul(_isometricReverseMatrix, new float4(screenPosition.x, screenPosition.y, 0F, 0F));
            return new float3(pos.x + screenPosition.z, pos.y - screenPosition.z, screenPosition.z);
        }

        public quaternion WorldToIso(quaternion worldRotation)
        {
            return math.mul(math.quaternion(_isometricMatrix), worldRotation);
        }

        public struct IsometricMatrixData
        {
            /// <summary>
            /// Offset from world position to isometric position
            /// </summary>
            public float3 OffsetWorldToIsometric;
            public float TileSize;
            public float TileRatio;
            public float TileAngle;
            public float TileHeight;
        }
    }
}