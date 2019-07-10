using Unity.Mathematics;
using UnityEngine;

namespace HMH.ECS.IsoSorting
{
    public static class Extensions
    {
        public static float3 ToFloat3(this float2 value, float zValue = 0F)
        {
            return new float3(value, zValue);
        }

        public static Vector3 ToVector3(this float2 value, float zValue = 0F)
        {
            return new Vector3(value.x, value.y, zValue);
        }
    }
}