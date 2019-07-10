#if ISOTOOL
using IsoTools;
using UnityEngine;

namespace HMH.ECS.IsoSorting
{
    public class ConvertIsoObjectManager : MonoBehaviour
    {
        private void Awake()
        {
            var isoObjs = FindObjectsOfType<IsoObject>();

            var root = new GameObject("IsoObject Converted");

            foreach (var obj in isoObjs)
            {
                var go = new GameObject(obj.transform.GetSiblingIndex().ToString());
                go.transform.parent        = root.transform;
                go.transform.localPosition = Vector3.zero;

                var spriteRenderer   = go.AddComponent<SpriteRenderer>();
                var originalRenderer = obj.GetComponent<SpriteRenderer>();

                if (originalRenderer)
                {
                    spriteRenderer.sprite = originalRenderer.sprite;
                    spriteRenderer.color  = originalRenderer.color;
                }

                var idg = go.AddComponent<IsometricDataGameobject>();

                var pos  = new Vector3(-obj.position.y, obj.position.x, obj.position.z) + _isometricOffset;
                var size = new Vector3(obj.size.y, obj.size.x, obj.size.z);

                idg.InitForConversion(pos, size, spriteRenderer);

                if (_destroyIsoObjectAfterConversion)
                    Destroy(obj);
            }
        }

#region Variables

        [SerializeField]
        private bool _destroyIsoObjectAfterConversion;
        [SerializeField]
        private Vector3 _isometricOffset;

#endregion
    }
}
#endif