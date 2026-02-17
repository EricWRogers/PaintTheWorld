#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;

namespace SimpleSurfaceAlignTool
{
    public enum UpAxis
    {
        X, Y, Z,
        NegativeX, NegativeY, NegativeZ
    }

    // Simple settings asset
    public class SimpleSurfaceAlignSettings : ScriptableObject
    {
        public UpAxis UpAxis = UpAxis.Y;
        public float RaycastDistance = 100f;
        public float DepthOffset = 0f;

        public static SimpleSurfaceAlignSettings GetOrCreateSettings()
        {
            const string path = "Assets/SimpleSurfaceAlignSettings.asset";
            var settings = AssetDatabase.LoadAssetAtPath<SimpleSurfaceAlignSettings>(path);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<SimpleSurfaceAlignSettings>();
                AssetDatabase.CreateAsset(settings, path);
                AssetDatabase.SaveAssets();
            }
            return settings;
        }
    }

    [EditorTool("Simple Surface Align Tool")]
    public class SurfaceSnappingTool : EditorTool
    {
        [SerializeField] private Texture2D _toolIcon;
        private GUIContent _iconContent;
        private SimpleSurfaceAlignSettings _settings;

        private Transform _activeTransform;

        private const int HANDLE_ID = 1;

        private void OnEnable()
        {
            _iconContent = new GUIContent()
            {
                image = _toolIcon,
                text = "Simple Surface Align Tool",
                tooltip = "Simple Surface Align Tool"
            };

            _settings = SimpleSurfaceAlignSettings.GetOrCreateSettings();
        }

        public override GUIContent toolbarIcon => _iconContent;

        // Optional shortcut: press S to activate the tool
        [UnityEditor.ShortcutManagement.Shortcut("Activate Simple Surface Align Tool", KeyCode.S)]
        private static void ActivateTool()
        {
            ToolManager.SetActiveTool<SurfaceSnappingTool>();
        }

        public override void OnToolGUI(EditorWindow window)
        {
            if (!(window is SceneView)) return;
            if (!ToolManager.IsActiveTool(this)) return;
            if (_settings == null) _settings = SimpleSurfaceAlignSettings.GetOrCreateSettings();

            // Get currently selected transform
            _activeTransform = Selection.activeTransform;
            if (_activeTransform == null) return;

            EditorGUI.BeginChangeCheck();

            // Handle rotation depends on global/local pivot
            Quaternion handleRotation = (Tools.pivotRotation == PivotRotation.Global)
                ? Quaternion.identity
                : _activeTransform.rotation;

            Vector3 targetPosition = Handles.PositionHandle(_activeTransform.position, handleRotation);

            Handles.color = Color.yellow;
            Handles.FreeMoveHandle(
                HANDLE_ID,
                _activeTransform.position,
                0.25f * HandleUtility.GetHandleSize(_activeTransform.position),
                Vector3.zero,
                Handles.SphereHandleCap);

            // If the free‑move handle is being dragged, raycast from mouse
            if (GUIUtility.hotControl == HANDLE_ID)
            {
                Vector3 hitNormal;
                Vector3 hitPoint = GetCurrentMousePositionInScene(out hitNormal);

                targetPosition = hitPoint + hitNormal * _settings.DepthOffset;
                Quaternion targetRotation = GetTargetRotation(hitNormal);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_activeTransform, "Simple Surface Align");
                    _activeTransform.position = targetPosition;
                    _activeTransform.rotation = targetRotation;
                    Physics.SyncTransforms();
                    window.Repaint();
                }
            }
            else
            {
                // Normal position handle behaviour (no snapping)
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_activeTransform, "Move");
                    _activeTransform.position = targetPosition;
                    Physics.SyncTransforms();
                    window.Repaint();
                }
            }

            DrawSimpleSettingsGUI();
        }

        private Vector3 GetCurrentMousePositionInScene(out Vector3 normal)
        {
            Vector2 mousePosition = Event.current.mousePosition;
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);

            int layerMask = ~LayerMask.GetMask("Ignore Raycast");
            float range = _settings.RaycastDistance;
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, range, layerMask))
            {
                normal = hit.normal;
                return hit.point;
            }

            // If nothing hit, just pick a point far along the ray
            normal = Vector3.up;
            return ray.origin + ray.direction * range;
        }

        private Quaternion GetTargetRotation(Vector3 normal)
        {
            Vector3 upAxis = GetUpAxisVector();
            return Quaternion.FromToRotation(upAxis, normal) * _activeTransform.rotation;
        }

        private Vector3 GetUpAxisVector()
        {
            switch (_settings.UpAxis)
            {
                case UpAxis.X:         return _activeTransform.right;
                case UpAxis.Y:         return _activeTransform.up;
                case UpAxis.Z:         return _activeTransform.forward;
                case UpAxis.NegativeX: return -_activeTransform.right;
                case UpAxis.NegativeY: return -_activeTransform.up;
                case UpAxis.NegativeZ: return -_activeTransform.forward;
                default:               return _activeTransform.up;
            }
        }

        private void DrawSimpleSettingsGUI()
        {
            Handles.BeginGUI();

            GUILayout.BeginArea(new Rect(10, 10, 250, 120), "Simple Surface Align", "Window");

            _settings.UpAxis = (UpAxis)EditorGUILayout.EnumPopup("Up Axis", _settings.UpAxis);
            _settings.RaycastDistance = EditorGUILayout.FloatField("Ray Distance", _settings.RaycastDistance);
            _settings.DepthOffset = EditorGUILayout.FloatField("Depth Offset", _settings.DepthOffset);

            if (GUI.changed)
            {
                EditorUtility.SetDirty(_settings);
            }

            GUILayout.EndArea();
            Handles.EndGUI();
        }
    }
}

#endif
