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

        for (int i = 0; i < points.arraySize; i++) {
            EditorGUI.BeginChangeCheck();
                    
            Handles.color = Color.red;
            Vector3 newPointPosition = Handles.Slider2D(blockPosition + points.GetArrayElementAtIndex(i).vector3Value, blockTransform.up, blockTransform.forward, blockTransform.right, 0.2f, Handles.CubeHandleCap, 1f);
            worldPositionPoints[i] = newPointPosition;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(block, "Change Point Position");
                points.GetArrayElementAtIndex(i).vector3Value = newPointPosition - blockPosition;
            }

            Handles.color = Color.yellow;
            int nextPointIndex = i < points.arraySize - 1 ? i + 1 : 0;
            Vector3 splitPointPosition = blockPosition + Vector3.Lerp(points.GetArrayElementAtIndex(nextPointIndex).vector3Value, points.GetArrayElementAtIndex(i).vector3Value, 0.5f);
            
            if (Handles.Button(splitPointPosition, Quaternion.identity, 0.2f, 0.2f, Handles.CubeHandleCap)) {
                Debug.Log("Split");
                points.InsertArrayElementAtIndex(i);
                points.GetArrayElementAtIndex(i).vector3Value = splitPointPosition;
            }
        }

        Handles.color = new Color(1f, 0f, 0f, 0.4f);
        Handles.DrawAAConvexPolygon(worldPositionPoints);

        serializedObject.ApplyModifiedProperties();
    }
}