using UnityEngine;
using UnityEditor;

public class LevelBlock : MonoBehaviour
{
    public float value = 7.0f;

    public Vector3[] points;
}

[CustomEditor(typeof(LevelBlock))]
public class LevelBlockEditor : Editor
{
    public void Awake() {
        var t = target as LevelBlock;
        t.points = new Vector3[] {
            new Vector3(2f, 0f, 2f),
            new Vector3(2f, 0f, -2f),
            new Vector3(-2f, 0f, -2f),
            new Vector3(-2f, 0f, 2f)
        };
    }

    public void OnSceneGUI()
    {
        var t = target as LevelBlock;
        var tr = t.transform;
        var pos = tr.position;

        Handles.color = Color.red;
        for (int i = 0; i < t.points.Length; i++) {
            EditorGUI.BeginChangeCheck();
            Vector3 newPointPosition = Handles.Slider2D(pos + t.points[i], tr.up, tr.forward, tr.right, 0.1f, Handles.CubeHandleCap, 1f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(t, "Change Point Position");
                t.points[i] = newPointPosition;
            }
        }

        Handles.color = new Color(1f, 0f, 0f, 0.4f);
        Handles.DrawAAConvexPolygon(t.points);
    }
}