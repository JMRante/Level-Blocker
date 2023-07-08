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

        LevelBlock block = target as LevelBlock;
        Transform blockTransform = block.transform;
        Vector3 blockPosition = block.transform.position;

        Vector3[] worldPositionPoints = new Vector3[points.arraySize];

        Handles.color = Color.red;
        for (int i = 0; i < points.arraySize; i++) {
            EditorGUI.BeginChangeCheck();
            
            Vector3 newPointPosition = Handles.Slider2D(blockPosition + points.GetArrayElementAtIndex(i).vector3Value, blockTransform.up, blockTransform.forward, blockTransform.right, 0.1f, Handles.CubeHandleCap, 1f);
            worldPositionPoints[i] = newPointPosition;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(block, "Change Point Position");
                points.GetArrayElementAtIndex(i).vector3Value = newPointPosition - blockPosition;
            }
        }

        Handles.color = new Color(1f, 0f, 0f, 0.4f);
        Handles.DrawAAConvexPolygon(worldPositionPoints);

        serializedObject.ApplyModifiedProperties();
    }
}