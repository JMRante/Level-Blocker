using UnityEngine;
using UnityEditor;
using Unity.Collections;
using UnityEngine.Rendering;
using Unity.Mathematics;

using static Unity.Mathematics.math;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class LevelBlock : MonoBehaviour
{
    public bool dirtyRender = true;

    public float bottomHeight = -2f; 
    public float topHeight = 2f;

    public Vector3[] points = new Vector3[] {
        new Vector3(2f, 0f, 2f),
        new Vector3(2f, 0f, -2f),
        new Vector3(-2f, 0f, -2f),
        new Vector3(-2f, 0f, 2f)
    };

    void Update() {
        if (dirtyRender) {
            int vertexAttributeCount = 4;
            int vertexCount = 4;
            int triangleIndexCount = 6;

            Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
            Mesh.MeshData meshData = meshDataArray[0];

            NativeArray<VertexAttributeDescriptor> vertexAttributes = new NativeArray<VertexAttributeDescriptor>(
                vertexAttributeCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory
            );

            vertexAttributes[0] = new VertexAttributeDescriptor(VertexAttribute.Position, dimension: 3, stream: 0);
            vertexAttributes[1] = new VertexAttributeDescriptor(VertexAttribute.Normal, dimension: 3, stream: 1);
            vertexAttributes[2] = new VertexAttributeDescriptor(VertexAttribute.Tangent, dimension: 4, stream: 2);
            vertexAttributes[3] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, dimension: 2, stream: 3);

            meshData.SetVertexBufferParams(vertexCount, vertexAttributes);
		    vertexAttributes.Dispose();

            NativeArray<float3> positions = meshData.GetVertexData<float3>();
            positions[0] = 0f;
            positions[1] = right();
            positions[2] = up();
            positions[3] = float3(1f, 1f, 0f);

            NativeArray<float3> normals = meshData.GetVertexData<float3>(1);
            normals[0] = normals[1] = normals[2] = normals[3] = back();

            NativeArray<float4> tangents = meshData.GetVertexData<float4>(2);
            tangents[0] = tangents[1] = tangents[2] = tangents[3] = float4(1f, 0f, 0f, -1f);

            NativeArray<float2> texCoords = meshData.GetVertexData<float2>(3);
            texCoords[0] = 0f;
            texCoords[1] = float2(1f, 0f);
            texCoords[2] = float2(0f, 1f);
            texCoords[3] = 1f;

            meshData.SetIndexBufferParams(triangleIndexCount, IndexFormat.UInt16);
            NativeArray<ushort> triangleIndices = meshData.GetIndexData<ushort>();
            triangleIndices[0] = 0;
            triangleIndices[1] = 2;
            triangleIndices[2] = 1;
            triangleIndices[3] = 1;
            triangleIndices[4] = 2;
            triangleIndices[5] = 3;

            meshData.subMeshCount = 1;
            meshData.SetSubMesh(0, new SubMeshDescriptor(0, triangleIndexCount));

            Mesh mesh = new Mesh {
                name = gameObject.name + " Level Block Mesh"
            };

            Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);

            GetComponent<MeshFilter>().mesh = mesh;

            dirtyRender = false;
        }
	}
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

            // For checking if a merge happened
            int newMergeIndexA = -1;
            int newMergeIndexB = -1;
            Vector3 newMergePoint = Vector3.zero;

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

                // Calculate values for merge and split
                int nextPointIndex = j < points.arraySize - 1 ? j + 1 : 0;
                Vector3 midPointPosition = Vector3.Lerp(points.GetArrayElementAtIndex(nextPointIndex).vector3Value, points.GetArrayElementAtIndex(j).vector3Value, 0.5f);

                // Line merge handles
                Handles.color = Color.green;

                if (Handles.Button(blockPosition + heightOffset + midPointPosition + (Vector3.up * 0.25f), Quaternion.Euler(90f, 0f, 0f), 0.15f, 0.15f, Handles.ConeHandleCap)) {
                    Undo.RecordObject(block, "Merge Between Two Points");
                    newMergeIndexA = j;
                    newMergeIndexB = nextPointIndex;
                    newMergePoint = midPointPosition;
                }                

                // Line split handles
                Handles.color = Color.blue;
                
                if (Handles.Button(blockPosition + heightOffset + midPointPosition, Quaternion.identity, 0.2f, 0.2f, Handles.SphereHandleCap)) {
                    Undo.RecordObject(block, "Split Between Two Points");
                    newSplitIndex = nextPointIndex;
                    newSplitPoint = midPointPosition;
                }

                // Only count points to center of mass once
                if (i == 0) {
                    centerOfMass += points.GetArrayElementAtIndex(j).vector3Value;
                }

                Handles.color = Color.black;
                Handles.Label(blockPosition + heightOffset + points.GetArrayElementAtIndex(j).vector3Value, j.ToString());
            } 

            // If a merge occured, apply it after the loop through of points
            if (newMergeIndexA != -1 && newMergeIndexB != -1 && points.arraySize > 3) {
                int mergedPointIndex = newMergeIndexB > newMergeIndexA ? newMergeIndexB : newMergeIndexA;

                points.InsertArrayElementAtIndex(mergedPointIndex);
                points.GetArrayElementAtIndex(mergedPointIndex).vector3Value = newMergePoint;

                points.DeleteArrayElementAtIndex(newMergeIndexB > newMergeIndexA ? newMergeIndexA : newMergeIndexA + 1);
                points.DeleteArrayElementAtIndex(newMergeIndexB);
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