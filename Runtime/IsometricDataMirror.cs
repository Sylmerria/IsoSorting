using HMH.ECS.SpatialHashing;

namespace HMH.ECS.IsoSorting
{
    public struct IsometricDataMirror : ISpatialHashingItemMiror
    {
        #region Implementation of ISpatialHashingItemMiror

        /// <inheritdoc />
        public int GetItemID { get; set; }

        #endregion
    }
}