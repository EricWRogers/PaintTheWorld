#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;

[EditorTool("Mesh Snap Tool")]
public class SurfaceSnappingTool : EditorTool
{
    [SerializeField] private Texture2D _icon;
    private GUIContent _iconContent;

    // Mesh we will snap onto
    public MeshFilter targetMeshFilter;

    private BaryCentricDistance _bary;

    private void OnEnable()
    {
        _iconContent = new GUIContent
        {
            image = _icon,
            text = "Mesh Snap",
            tooltip = "Snap selected object to closest point on target mesh"
        };
    }

    public override GUIContent toolbarIcon => _iconContent;

    public override void OnToolGUI(EditorWindow window)
    {
        if (!(window is SceneView)) return;

        // Need a selection and a target mesh
        var selected = Selection.activeTransform;
        if (!selected || !targetMeshFilter || !targetMeshFilter.sharedMesh)
            return;

        if (_bary == null || _baryMeshChanged())
            _bary = new BaryCentricDistance(targetMeshFilter);

        Handles.BeginGUI();
        GUILayout.BeginArea(new Rect(10, 10, 250, 80), "Mesh Snap Tool", "Window");

        EditorGUILayout.ObjectField("Target Mesh",
            targetMeshFilter, typeof(MeshFilter), true);

        if (GUILayout.Button("Snap Selected To Mesh"))
        {
            SnapSelectedToMesh(selected);
        }

        GUILayout.EndArea();
        Handles.EndGUI();
    }

    private bool _baryMeshChanged()
    {
        return false;
    }

    private void SnapSelectedToMesh(Transform selected)
    {
        if (_bary == null) return;

        // Use object pivot as query point
        Vector3 queryPoint = selected.position;

        var result = _bary.GetClosestTriangleAndPoint(queryPoint);

        // If no valid triangle, bail
        if (float.IsInfinity(result.distanceSquared))
            return;

        Undo.RecordObject(selected, "Snap To Mesh");

        // Move to closest point
        selected.position = result.closestPoint;

        // Optional: align up axis to triangle normal (Y up here)
        if (result.normal != Vector3.zero)
        {
            Quaternion rot = Quaternion.FromToRotation(selected.up, result.normal) * selected.rotation;
            selected.rotation = rot;
        }

        // Ensure physics transforms update
        Physics.SyncTransforms();
    }
}
#endif