using UnityEditor;
using UnityEngine;

public class SurfaceSnapTool : EditorWindow
{
    float rayDistance = 100f;
    float offset = 0f;
    LayerMask snapLayers = ~0;
    bool alignRotation = true;

    [MenuItem("Tools/Surface Snap Tool")]
    public static void ShowWindow()
    {
        GetWindow<SurfaceSnapTool>("Surface Snap");
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Surface Snap Settings", EditorStyles.boldLabel);
        rayDistance    = EditorGUILayout.FloatField("Ray Distance", rayDistance);
        offset         = EditorGUILayout.FloatField("Offset", offset);

        // This draws the standard LayerMask dropdown
        snapLayers = EditorGUILayout.LayerField("Snap Layer", snapLayers);

        alignRotation  = EditorGUILayout.Toggle("Align Rotation", alignRotation);

        EditorGUILayout.Space();

        if (GUILayout.Button("Snap Selected To Surface"))
        {
            SnapSelection();
        }
    }

    void SnapSelection()
    {
        foreach (var obj in Selection.transforms)
        {
            SnapTransform(obj);
        }
    }

    void SnapTransform(Transform t)
    {
        Vector3 origin = t.position;
        Vector3 direction = Vector3.down;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, rayDistance, snapLayers))
        {
            Undo.RecordObject(t, "Surface Snap");

            t.position = hit.point + hit.normal * offset;

            if (alignRotation)
            {
                Vector3 up = hit.normal;
                Vector3 forward = Vector3.ProjectOnPlane(t.forward, up).normalized;
                if (forward.sqrMagnitude < 0.0001f)
                    forward = Vector3.ProjectOnPlane(Vector3.forward, up).normalized;
                t.rotation = Quaternion.LookRotation(forward, up);
            }
        }
    }
}
