using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace HMH.ECS.IsoSorting
{
    public class SpawnIsoData : MonoBehaviour
    {
        public int         Width  = 100;
        public int         Height = 400;
        public LayerData[] Layers;

        public  GameObject  ModelGameObject;
        public  bool        _showGizmo;
        private Matrix4x4[] _matrixArray;
        public  Material    _materialDeubg;
        public  Mesh        _meshDebug;

        // Use this for initialization
        void Start()
        {
            if (ModelGameObject != null)
            {
                for (int l = 0; l < Layers.Length; l++)
                {
                    var layer = Layers[l];

                    int arrayIndex = 0;

                    for (int x = 0; x < Width; x++)
                    {
                        for (int y = 0; y < Height; y++)
                        {
                            var isoOOP = Instantiate(ModelGameObject).GetComponent<IsometricDataGameobject>();
                            isoOOP.Position        = new Vector3(x, y, 0);
                            isoOOP.Size            = layer.Size;
                            isoOOP.gameObject.name = (x * Height + y).ToString();
                        }
                    }
                }

                return;
            }

            var matrixList = new List<Matrix4x4>(1 << 15);
            var em         = World.Active.EntityManager;

            var matrix = World.Active.GetOrCreateSystem<IsometricSyncSystem>().GetSingleton<IsometricMatrix>();

            for (int l = 0; l < Layers.Length; l++)
            {
                var layer = Layers[l];

                var entityArray = new NativeArray<Entity>(Width * Height, Allocator.Temp);
                em.CreateEntity(em.CreateArchetype(typeof(IsometricData), typeof(IsometricDepthData), typeof(IsometricDataNeedUpdate)), entityArray);

                int arrayIndex = 0;

                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        em.SetComponentData(entityArray[arrayIndex], new IsometricData { IsoPosition = new float3(x, y, 0F), IsoSize = layer.Size, Entity = entityArray[arrayIndex++] });

                        matrixList.Add(Matrix4x4.TRS(matrix.IsoToScreen(new float3(x, y, 0F)).ToVector3() +
                                                     new Vector3(0F, matrix.IsometricData.TileSize * matrix.IsometricData.TileRatio / 2, 0F),
                                                     Quaternion.identity,
                                                     new Vector3(matrix.IsometricData.TileSize * 2, matrix.IsometricData.TileSize * 2 * matrix.IsometricData.TileRatio, 1F)));
                    }
                }
            }

            _matrixArray = matrixList.ToArray();
        }

#if UNITY_EDITOR
        private void LateUpdate()
        {
            if (_showGizmo)
                foreach (var m in _matrixArray)
                {
                    Graphics.DrawMesh(_meshDebug, m, _materialDeubg, 0);
                }
        }
#endif

        [System.Serializable]
        public struct LayerData
        {
            public Vector3 Size;
        }
    }
}