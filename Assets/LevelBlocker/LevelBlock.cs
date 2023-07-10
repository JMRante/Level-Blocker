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

        // Get references to targeted level block
        LevelBlock block = target as LevelBlock;
        Transform blockTransform = block.transform;
        Vector3 blockPosition = block.transform.position;

        Vector3[] worldPositionPoints = new Vector3[points.arraySize + 1];

        // For checking if a split happened
        int newSplitIndex = -1;
        Vector3 newSplitPoint = Vector3.zero;

        // Iterate through all block points, drawing and checking their handles
        for (int i = 0; i < points.arraySize; i++) {
            EditorGUI.BeginChangeCheck();
                    
            Handles.color = Color.red;
            Vector3 newPointPosition = Handles.Slider2D(blockPosition + points.GetArrayElementAtIndex(i).vector3Value, blockTransform.up, blockTransform.forward, blockTransform.right, 0.3f, Handles.CubeHandleCap, 1f);
            worldPositionPoints[i] = newPointPosition;

            if (i == 0) {
                // Add first point again to loop polyline
                worldPositionPoints[points.arraySize] = newPointPosition;
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(block, "Change Point Position");
                points.GetArrayElementAtIndex(i).vector3Value = newPointPosition - blockPosition;
            }

            Handles.color = Color.blue;
            int nextPointIndex = i < points.arraySize - 1 ? i + 1 : 0;
            Vector3 splitPointPosition = Vector3.Lerp(points.GetArrayElementAtIndex(nextPointIndex).vector3Value, points.GetArrayElementAtIndex(i).vector3Value, 0.5f);
            
            if (Handles.Button(blockPosition + splitPointPosition, Quaternion.identity, 0.2f, 0.2f, Handles.SphereHandleCap)) {
                Undo.RecordObject(block, "Split Between Two Points");
                newSplitIndex = nextPointIndex;
                newSplitPoint = splitPointPosition;
            }
        }

        if (newSplitIndex != -1) {
            points.InsertArrayElementAtIndex(newSplitIndex);
            points.GetArrayElementAtIndex(newSplitIndex).vector3Value = newSplitPoint;
        }

        Handles.color = new Color(1f, 0f, 0f, 0.4f);
        Handles.DrawAAPolyLine(10f, worldPositionPoints);

        serializedObject.ApplyModifiedProperties();
    }
}