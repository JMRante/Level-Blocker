using UnityEngine;
using UnityEditor;

public class LevelBlock : MonoBehaviour
{
    public float bottomHeight = -2f; 
    public float topHeight = 2f;

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
    private const float MIN_HEIGHT = 0.2f;

    private SerializedProperty bottomHeight;
    private SerializedProperty topHeight;

    private SerializedProperty points;

    public void OnEnable() {
        bottomHeight = serializedObject.FindProperty("bottomHeight");
        topHeight = serializedObject.FindProperty("topHeight");

        points = serializedObject.FindProperty("points");
    }

    public void OnSceneGUI()
    {
        serializedObject.Update();

        // Get references to targeted level block
        LevelBlock block = target as LevelBlock;
        Transform blockTransform = block.transform;
        Vector3 blockPosition = block.transform.position;

        // For setting position of height handles
        Vector3 centerOfMass = Vector3.zero;

        // Create and draw handles for both bottom and top of level block
        for (int i = 0; i <= 1; i++) {
            Vector3 heightOffset = i == 0 ? Vector3.up * bottomHeight.floatValue : Vector3.up * topHeight.floatValue;

            Vector3[] worldPositionPoints = new Vector3[points.arraySize + 1];

            // For checking if a split happened
            int newSplitIndex = -1;
            Vector3 newSplitPoint = Vector3.zero;

            // Iterate through all block points, drawing and checking their handles
            for (int j = 0; j < points.arraySize; j++) {
                // Point position handles
                EditorGUI.BeginChangeCheck();
                        
                Handles.color = Color.red;
                Vector3 newPointPosition = Handles.Slider2D(blockPosition + heightOffset + points.GetArrayElementAtIndex(j).vector3Value, blockTransform.up, blockTransform.forward, blockTransform.right, 0.3f, Handles.CubeHandleCap, 1f);
                worldPositionPoints[j] = newPointPosition;

                if (j == 0) {
                    // Add first point again to loop polyline
                    worldPositionPoints[points.arraySize] = newPointPosition;
                }

                // Once done drawing and storing points at height offset, restore before offset to serialize the value in 2D at the base of the block
                newPointPosition -= heightOffset;

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(block, "Change Point Position");
                    points.GetArrayElementAtIndex(j).vector3Value = newPointPosition - blockPosition;
                }

                // Line split handles
                Handles.color = Color.blue;
                int nextPointIndex = j < points.arraySize - 1 ? j + 1 : 0;
                Vector3 splitPointPosition = Vector3.Lerp(points.GetArrayElementAtIndex(nextPointIndex).vector3Value, points.GetArrayElementAtIndex(j).vector3Value, 0.5f);
                
                if (Handles.Button(blockPosition + heightOffset + splitPointPosition, Quaternion.identity, 0.2f, 0.2f, Handles.SphereHandleCap)) {
                    Undo.RecordObject(block, "Split Between Two Points");
                    newSplitIndex = nextPointIndex;
                    newSplitPoint = splitPointPosition;
                }

                // Only count points to center of mass once
                if (i == 0) {
                    centerOfMass += points.GetArrayElementAtIndex(j).vector3Value;
                }
            } 

            // If a split occured, apply it after the loop through of points
            if (newSplitIndex != -1) {
                points.InsertArrayElementAtIndex(newSplitIndex);
                points.GetArrayElementAtIndex(newSplitIndex).vector3Value = newSplitPoint;
            }

            // Draw outline of shape the points create
            Handles.color = new Color(1f, 0f, 0f, 0.4f);
            Handles.DrawAAPolyLine(10f, worldPositionPoints);
        }

        // Prepare to render height sliders
        Handles.color = Color.magenta;
        centerOfMass = centerOfMass / (float) points.arraySize;
        centerOfMass.y = blockPosition.y;
        Vector3 blockPositionNoY = new Vector3(blockPosition.x, 0f, blockPosition.z);

        // Bottom height slider
        EditorGUI.BeginChangeCheck();
        Vector3 newBottomPosition = Handles.Slider(centerOfMass + blockPositionNoY + (Vector3.up * bottomHeight.floatValue), Vector3.down, 3f, Handles.ArrowHandleCap, 1f);
        float newBottomHeight = newBottomPosition.y > topHeight.floatValue - MIN_HEIGHT ? topHeight.floatValue - MIN_HEIGHT : newBottomPosition.y;

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Change Bottom Height");
            bottomHeight.floatValue = newBottomHeight;
        }

        // Top height slider
        EditorGUI.BeginChangeCheck();
        Vector3 newTopPosition = Handles.Slider(centerOfMass + blockPositionNoY + (Vector3.up * topHeight.floatValue), Vector3.up, 3f, Handles.ArrowHandleCap, 1f);
        float newTopHeight = newTopPosition.y < bottomHeight.floatValue + MIN_HEIGHT ? bottomHeight.floatValue + MIN_HEIGHT : newTopPosition.y;

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Change Top Height");
            topHeight.floatValue = newTopHeight;
        }

        serializedObject.ApplyModifiedProperties();
    }
}