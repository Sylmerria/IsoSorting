using HMH.ECS.SpatialHashing;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.Assertions;

namespace HMH.ECS.IsoSorting
{
    public struct IsometricData : IComponentData, ISpatialHashingItem<IsometricData>
    {
        #region Variables

        /// <summary> Need tag IsometrixDataNeedUpdate to be set directly </summary>
        public float3 IsoPosition;
        /// <summary> Need tag IsometrixDataNeedUpdate to be set directly </summary>
        public float3 IsoSize;

        public float2 ScreenMin;
        public float2 ScreenMax;

        public Entity Entity;

        public int SpatialHashingIndexInternal;

        #endregion

        #region Properties

        public float3 IsoMin => new float3(IsoPosition.x - IsoSize.x, IsoPosition.yz);
        public float3 IsoMax => new float3(IsoPosition.x, IsoPosition.yz + IsoSize.yz);

        #region Implementation of ISpatialHashingItem<IsoData>

        /// <inheritdoc />
        public int SpatialHashingIndex { get => SpatialHashingIndexInternal; set => SpatialHashingIndexInternal = value; }

        #endregion

        #endregion

        public void Init(float3 isoPosition, float3 isoSize, IsometricMatrix matrix)
        {
            IsoPosition = isoPosition;
            IsoSize     = isoSize;

            UpdateScreenPosition(matrix);
        }

        public void UpdatePosition(float3 isoPosition, IsometricMatrix matrix)
        {
            IsoPosition = isoPosition;

            UpdateScreenPosition(matrix);
        }

        public void UpdateSize(float3 isoSize, IsometricMatrix matrix)
        {
            IsoSize = isoSize;

            UpdateScreenPosition(matrix);
        }

        public void UpdateScreenPosition(IsometricMatrix matrix)
        {
            var bottomPosition = matrix.IsoToScreen(IsoPosition);
            var topPosition    = matrix.IsoToScreen(new float3( IsoPosition.x -IsoSize.x,IsoPosition.yz + IsoSize.yz));
            var isoBottom      = IsoPosition;
            isoBottom.y += IsoSize.y;
            var rightPosition = matrix.IsoToScreen(isoBottom);
            var leftPosition = matrix.IsoToScreen(new float3(IsoPosition.x - IsoSize.x, IsoPosition.yz));

            var tempMin=new float2(leftPosition.x, bottomPosition.y);
            var tempMax=new float2(rightPosition.x, topPosition.y);

            ScreenMin = math.min(tempMin, tempMax);
            ScreenMax = math.max(tempMin, tempMax);
        }

        #region Implementation of ISpatialHashingItem<IsoData>

        /// <inheritdoc />
        public float3 GetCenter()
        {
            return new float3(ScreenMin, 0.5F) + GetSize();
        }

        /// <inheritdoc />
        public float3 GetSize()
        {
            return new float3((ScreenMax.x - ScreenMin.x) * 0.5F, (ScreenMax.y - ScreenMin.y) * 0.5F, 0.1F);
        }

        #endregion

        #region Implementation of IEquatable<IsoData>

        /// <inheritdoc />
        public bool Equals(IsometricData other)
        {
            Assert.AreNotEqual(0, SpatialHashingIndex);
            return SpatialHashingIndex == other.SpatialHashingIndex;
        }

        #region Overrides of ValueType

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return SpatialHashingIndex;
        }

        #endregion

        #endregion

        #region Overrides of ValueType

        /// <inheritdoc />
        public override string ToString()
        {
            return $"ID {SpatialHashingIndexInternal} Pos {IsoPosition} Size {IsoSize}";
        }

        #endregion
    }

    public struct IsometricDataNeedUpdate : IComponentData
    { }
}