using UnityEngine;
using UnityEditor;

public class LevelBlock : MonoBehaviour
{
    public Vector3[] points = new Vector3[] {
        new Vector3(2f, 0f, 2f),
        new Vector3(2f, 0f, -2f),
        new Vector3(-2f, 0f, -2f),
        new Vector3(-2f, 0f, 2f)
    };
}

// Investigate
// https://docs.unity3d.com/ScriptReference/SerializedProperty.html
// https://docs.unity3d.com/ScriptReference/ScriptableObject.html 
[CustomEditor(typeof(LevelBlock))]
public class LevelBlockEditor : Editor
{
    private SerializedProperty points;

    public void OnEnable() {
        points = serializedObject.FindProperty("points");
    }

    public void OnSceneGUI()
    {
        serializedObject.Update();

        var t = target as LevelBlock;
        var tr = t.transform;
        var pos = tr.position;

        Handles.color = Color.red;
        for (int i = 0; i < points.arraySize; i++) {
            EditorGUI.BeginChangeCheck();
            Vector3 newPointPosition = Handles.Slider2D(pos + points.GetArrayElementAtIndex(i).vector3Value, tr.up, tr.forward, tr.right, 0.1f, Handles.CubeHandleCap, 1f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(t, "Change Point Position");
                points.GetArrayElementAtIndex(i).vector3Value = newPointPosition;
            }
        }

        Handles.color = new Color(1f, 0f, 0f, 0.4f);
        Handles.DrawAAConvexPolygon(t.points);

        serializedObject.ApplyModifiedProperties();
    }
}