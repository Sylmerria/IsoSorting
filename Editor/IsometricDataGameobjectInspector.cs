using Unity.Entities;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace HMH.ECS.IsoSorting.Editor
{
    [CustomEditor(typeof(IsometricDataGameobject))]
    public class IsometricDataGameobjectInspector : UnityEditor.Editor
    {
        public void OnEnable()
        {
            // Import UXML
            _moduleVisualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/IsometricDataGameobjectInspector.uxml");
        }

        private void OnDisable()
        {
            if (Application.isPlaying == false)
                _matrix = IsometricMatrixData.CreateIsometricMatrix(Resources.Load<IsometricMatrixData>("IsometricMatrixData"));
        }

        /// <inheritdoc />
        public override VisualElement CreateInspectorGUI()
        {
            if (Application.isPlaying == false)
                _matrix = IsometricMatrixData.CreateIsometricMatrix(Resources.Load<IsometricMatrixData>("IsometricMatrixData"));
            else
                _matrix = World.Active.GetExistingSystem<IsometricSyncSystem>().GetSingleton<IsometricMatrix>();

            _root = new VisualElement();
            _root.Add(_moduleVisualTree.CloneTree());

            var position = _root.Q<VisualElement>("Position");
            position.RegisterCallback<ChangeEvent<Vector3>>(PositionChanged);

            var sizeElement = _root.Q<VisualElement>("Size");
            sizeElement.RegisterCallback<ChangeEvent<Vector3>>(SizeChanged);

            return _root;
        }

        private void PositionChanged(ChangeEvent<Vector3> evt)
        {
            if (_matrix.HasValue == false)
                return;

            _localDataAreDirty = false;

            var isoTarget = (IsometricDataGameobject)target;

            isoTarget.transform.position = _matrix.Value.IsoToScreen(isoTarget.Position).ToVector3();

            if (Application.isPlaying)
                isoTarget.UpdateLocalDataToWorldData();
        }

        private void SizeChanged(ChangeEvent<Vector3> evt)
        {
            if (_matrix.HasValue == false)
                return;

            _localDataAreDirty = false;

            var isoTarget = (IsometricDataGameobject)target;

            isoTarget.transform.position = _matrix.Value.IsoToScreen(isoTarget.Position).ToVector3();

            if (Application.isPlaying)
                isoTarget.UpdateLocalDataToWorldData();
        }

        #region Variables

        private VisualTreeAsset _moduleVisualTree;

        private VisualElement _root;

        private SerializedProperty _positionPropertyField;
        private IsometricMatrix?   _matrix;

        private bool _localDataAreDirty;

        #endregion
    }
}