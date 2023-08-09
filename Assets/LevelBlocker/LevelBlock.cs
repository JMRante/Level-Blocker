using UnityEngine;
using UnityEditor;
using Unity.Collections;
using UnityEngine.Rendering;
using Unity.Mathematics;
using System;

using static Unity.Mathematics.math;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class LevelBlock : MonoBehaviour {
    public bool dirtyMesh = true;

    public float bottomHeight = -2f; 
    public float topHeight = 2f;

    [Range(0.001f, 0.5f)]
    public float virtualBevelHalfWidth = 0.1f;

    public Vector3[] points = new Vector3[] {
        new Vector3(2f, 0f, 2f),
        new Vector3(2f, 0f, -2f),
        new Vector3(-2f, 0f, -2f),
        new Vector3(-2f, 0f, 2f)
    };

    void Update() {
        if (dirtyMesh) {
            LevelBlockData geometryData = new LevelBlockData(points, bottomHeight, topHeight, virtualBevelHalfWidth);

            // For calculating bounds
            Vector3 boundingCenter = Vector3.zero;
            Vector3 boundingSize = Vector3.zero;

            Vector3 maxBoundPoint = points[0];
            Vector3 minBoundPoint = points[0];

            foreach (Vector3 point in points) {
                if (point.x > maxBoundPoint.x) maxBoundPoint.x = point.x;
                if (point.x < minBoundPoint.x) minBoundPoint.x = point.x;

                if (point.z > maxBoundPoint.z) maxBoundPoint.z = point.z;
                if (point.z < minBoundPoint.z) minBoundPoint.z = point.z;
            }

            maxBoundPoint.y = topHeight;
            minBoundPoint.y = bottomHeight;

            boundingSize = maxBoundPoint - minBoundPoint;
            boundingCenter = (minBoundPoint + maxBoundPoint) / 2f;

            // Mesh data sizes
            int vertexAttributeCount = 3;
            int vertexCount = geometryData.vertices.Count;
            int triangleIndexCount = geometryData.triangles.Count * 3;

            // Mesh data schema creation and allocation
            Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
            Mesh.MeshData meshData = meshDataArray[0];

            NativeArray<VertexAttributeDescriptor> vertexAttributes = new NativeArray<VertexAttributeDescriptor>(
                vertexAttributeCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory
            );

            vertexAttributes[0] = new VertexAttributeDescriptor(VertexAttribute.Position, dimension: 3, stream: 0);
            vertexAttributes[1] = new VertexAttributeDescriptor(VertexAttribute.Normal, dimension: 3, stream: 1);
            vertexAttributes[2] = new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float16, 4, 2);

            meshData.SetVertexBufferParams(vertexCount, vertexAttributes);
		    vertexAttributes.Dispose();

            half h0 = half(0f);
            half h1 = half(1f);

            // Set mesh data per point
            NativeArray<float3> positions = meshData.GetVertexData<float3>();
            NativeArray<float3> normals = meshData.GetVertexData<float3>(1);
            NativeArray<half4> tangents = meshData.GetVertexData<half4>(2);
            meshData.SetIndexBufferParams(triangleIndexCount, IndexFormat.UInt16);
            NativeArray<ushort> triangleIndices = meshData.GetIndexData<ushort>();

            for (int i = 0; i < geometryData.vertices.Count; i++) {
                Vertex vertex = geometryData.vertices[i];

                positions[i] = vertex.Position;
                normals[i] = vertex.Normal;
                tangents[i] = half4(half3(vertex.Tangent), half(-1f));
            }

            for (int i = 0; i < geometryData.triangles.Count; i++) {
                Triangle triangle = geometryData.triangles[i];
                int baseIndex = i * 3;

                triangleIndices[baseIndex] = Convert.ToUInt16(triangle.VertexA.Index);
                triangleIndices[baseIndex + 1] = Convert.ToUInt16(triangle.VertexB.Index);
                triangleIndices[baseIndex + 2] = Convert.ToUInt16(triangle.VertexC.Index);
            }

            Bounds bounds = new Bounds(boundingCenter, boundingSize);

            meshData.subMeshCount = 1;
            meshData.SetSubMesh(0, new SubMeshDescriptor(0, triangleIndexCount){
			    bounds = bounds,
			    vertexCount = vertexCount
            }, MeshUpdateFlags.DontRecalculateBounds);

            Mesh mesh = new Mesh {
                bounds = bounds,
                name = gameObject.name + " Level Block Mesh"
            };

            Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);

            bool successfulLightingUVGeneration = Unwrapping.GenerateSecondaryUVSet(mesh);

            if (!successfulLightingUVGeneration) {
                Debug.LogError("Failed to generate lighting UVs");
            }

            GetComponent<MeshFilter>().mesh = mesh;

            dirtyMesh = false;
        }
	}

    // Render bounds
    // void OnDrawGizmos()
    // {
    //     MeshFilter mf = GetComponent<MeshFilter>();

    //     Gizmos.color = new Color(0, 0, 1, 0.5f);
    //     Gizmos.DrawCube(transform.position + mf.sharedMesh.bounds.center, mf.sharedMesh.bounds.extents * 2);
    // }
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

        Undo.undoRedoPerformed += UndoRedoLevelBlockCallback;
    }

    public void OnSceneGUI() {
        serializedObject.Update();

        // Get references to targeted level block
        LevelBlock block = target as LevelBlock;
        Transform blockTransform = block.transform;

        // For setting position of height handles
        Vector3 centerOfMass = Vector3.zero;

        // Set transform matrix for Handles to be local 
        Handles.matrix = blockTransform.localToWorldMatrix;

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
                        
                Handles.color = Color.magenta;
                Vector3 newPointPosition = Handles.Slider2D(heightOffset + points.GetArrayElementAtIndex(j).vector3Value, Vector3.up, Vector3.forward, Vector3.right, 0.3f, Handles.CubeHandleCap, 1f);
                worldPositionPoints[j] = newPointPosition;

                if (j == 0) {
                    // Add first point again to loop polyline
                    worldPositionPoints[points.arraySize] = newPointPosition;
                }

                // Once done drawing and storing points at height offset, restore before offset to serialize the value in 2D at the base of the block
                newPointPosition -= heightOffset;

                if (EditorGUI.EndChangeCheck()) {
                    Undo.RecordObject(block, "Change Point Position");
                    points.GetArrayElementAtIndex(j).vector3Value = newPointPosition;
                    block.dirtyMesh = true;
                }

                // Calculate values for merge and split
                int nextPointIndex = j < points.arraySize - 1 ? j + 1 : 0;
                Vector3 midPointPosition = Vector3.Lerp(points.GetArrayElementAtIndex(nextPointIndex).vector3Value, points.GetArrayElementAtIndex(j).vector3Value, 0.5f);

                // Line merge handles
                Handles.color = Color.red;

                if (Handles.Button(heightOffset + midPointPosition + (Vector3.up * 0.25f), Quaternion.Euler(90f, 0f, 0f), 0.15f, 0.15f, Handles.ConeHandleCap)) {
                    Undo.RecordObject(block, "Merge Between Two Points");
                    newMergeIndexA = j;
                    newMergeIndexB = nextPointIndex;
                    newMergePoint = midPointPosition;
                }                

                // Line split handles
                Handles.color = Color.green;
                
                if (Handles.Button(heightOffset + midPointPosition, Quaternion.identity, 0.2f, 0.2f, Handles.SphereHandleCap)) {
                    Undo.RecordObject(block, "Split Between Two Points");
                    newSplitIndex = nextPointIndex;
                    newSplitPoint = midPointPosition;
                }

                // Only count points to center of mass once
                if (i == 0) {
                    centerOfMass += points.GetArrayElementAtIndex(j).vector3Value;
                }
            } 

            // If a merge occured, apply it after the loop through of points
            if (newMergeIndexA != -1 && newMergeIndexB != -1 && points.arraySize > 3) {
                int mergedPointIndex = newMergeIndexB > newMergeIndexA ? newMergeIndexB : newMergeIndexA;

                points.InsertArrayElementAtIndex(mergedPointIndex);
                points.GetArrayElementAtIndex(mergedPointIndex).vector3Value = newMergePoint;

                points.DeleteArrayElementAtIndex(newMergeIndexB > newMergeIndexA ? newMergeIndexA : newMergeIndexA + 1);
                points.DeleteArrayElementAtIndex(newMergeIndexB);
                block.dirtyMesh = true;
            }

            // If a split occured, apply it after the loop through of points
            if (newSplitIndex != -1) {
                points.InsertArrayElementAtIndex(newSplitIndex);
                points.GetArrayElementAtIndex(newSplitIndex).vector3Value = newSplitPoint;
                block.dirtyMesh = true;
            }

            // Draw outline of shape the points create
            Handles.color = Color.magenta;
            Handles.DrawAAPolyLine(10f, worldPositionPoints);
        }

        // Prepare to render height sliders
        Handles.color = Color.magenta;
        centerOfMass = centerOfMass / (float) points.arraySize;

        // Bottom height slider
        EditorGUI.BeginChangeCheck();
        Vector3 newBottomPosition = Handles.Slider(centerOfMass + (Vector3.up * bottomHeight.floatValue), Vector3.down, 3f, Handles.ArrowHandleCap, 1f);
        float newBottomHeight = newBottomPosition.y > topHeight.floatValue - MIN_HEIGHT ? topHeight.floatValue - MIN_HEIGHT : newBottomPosition.y;

        if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(target, "Change Bottom Height");
            bottomHeight.floatValue = newBottomHeight;
            block.dirtyMesh = true;
        }

        // Top height slider
        EditorGUI.BeginChangeCheck();
        Vector3 newTopPosition = Handles.Slider(centerOfMass + (Vector3.up * topHeight.floatValue), Vector3.up, 3f, Handles.ArrowHandleCap, 1f);
        float newTopHeight = newTopPosition.y < bottomHeight.floatValue + MIN_HEIGHT ? bottomHeight.floatValue + MIN_HEIGHT : newTopPosition.y;

        if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(target, "Change Top Height");
            topHeight.floatValue = newTopHeight;
            block.dirtyMesh = true;
        }

        serializedObject.ApplyModifiedProperties();

        Handles.matrix = Matrix4x4.identity;
    }

    void UndoRedoLevelBlockCallback()
    {
        LevelBlock block = target as LevelBlock;
        block.dirtyMesh = true;
    }
}