using System;
using System.Collections.Concurrent;
using UnityEngine;

#if UNITY_EDITOR
namespace HMH.ECS.IsoSorting
{
    public class GizmoDrawerHelper : MonoBehaviour
    {
        private void Awake()
        {
            hideFlags = HideFlags.HideAndDontSave;
            _instance = this;
        }

        public void Add(Action action)
        {
            _gizmoToDrawThisUpdate.Enqueue(action);
        }

        private void OnDrawGizmos()
        {
            while (_gizmoToDrawThisUpdate.TryDequeue(out var action))
                action();
        }

        #region Variables

        private static GizmoDrawerHelper _instance;

        private ConcurrentQueue<Action> _gizmoToDrawThisUpdate = new ConcurrentQueue<Action>();

        #endregion

        #region Properties

        public static GizmoDrawerHelper Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new GameObject("Gizmo Drawer Helper").AddComponent<GizmoDrawerHelper>();

                return _instance;
            }
        }

        #endregion
    }
}
#endif