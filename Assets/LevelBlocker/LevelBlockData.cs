using UnityEngine;
using System.Collections.Generic;

public class LevelBlockData
{
    public List<Vertex> vertices;
    public List<Triangle> triangles;

    public LevelBlockData(Vector3[] points, float bottomHeight, float topHeight, float virtualBevelHalfWidth) {
        // Calculate Quad Triangle counts for border sections
        int nQuadsInSide = 9;
        int nQuadsOnCap = 3;
        int nQuadsInBorderSection = nQuadsInSide + (nQuadsOnCap * 2);
        int nQuadsInBlock = nQuadsInBorderSection * points.Length;
        int nTrianglesInQuads = nQuadsInBlock * 2;

        // Calculate polygon triangles for caps
        int nTrianglesInPolygon = points.Length - 2;
        int nTrianglesInBlock = nTrianglesInPolygon * 2;

        int nPointsInPolygonChain = points.Length * 3;

        // Calculate data section sizes
        int sideSectionDataSize = 12;
        int sideDataSize = points.Length * sideSectionDataSize; 

        int capBorderSectionDataSize = 6;
        int capBorderDataSize = points.Length * capBorderSectionDataSize;

        // Init vertex/triangle lists and polygon vertex chains
        triangles = new List<Triangle>(nTrianglesInBlock + nTrianglesInQuads);
        vertices = new List<Vertex>(triangles.Capacity * 3);

        List<Vertex> bottomPolygonVertexChain = new List<Vertex>(nPointsInPolygonChain);
        List<Vertex> earVertices = new List<Vertex>(nPointsInPolygonChain);

        // Precalculate vertex side point height offsets
        Vector3 offset1 = Vector3.up * bottomHeight;
        Vector3 offset2 = (Vector3.up * bottomHeight) + (Vector3.up * virtualBevelHalfWidth);
        Vector3 offset3 = (Vector3.up * topHeight) + (Vector3.down * virtualBevelHalfWidth);
        Vector3 offset4 = Vector3.up * topHeight;

        // Create main side vertexes
        for (int i = 0; i < points.Length; i++) {
            int baseIndex = i * 12;

            Vector3 anchorPoint = points[i];
            Vector3 previousPoint = points[i - 1 == -1 ? points.Length - 1 : i - 1];
            Vector3 nextPoint = points[(i + 1) % points.Length];
            Vector3 side = nextPoint - anchorPoint;

            // Column A
            Vertex vertexA1 = new Vertex(anchorPoint + offset1, baseIndex);
            Vertex vertexA2 = new Vertex(anchorPoint + offset2, baseIndex + 1);
            Vertex vertexA3 = new Vertex(anchorPoint + offset3, baseIndex + 2);
            Vertex vertexA4 = new Vertex(anchorPoint + offset4, baseIndex + 3);

            // Calculate column A normals
            // Get vector between previous point and next point
            Vector3 transitionDirection = (nextPoint - previousPoint).normalized;

            // Get left hand perpendicular vector to the above
            Vector3 normalDirection = new Vector3(-transitionDirection.z, 0f, transitionDirection.x);

            // Set normals
            vertexA1.Normal = normalDirection.normalized + Vector3.down;
            vertexA2.Normal = normalDirection.normalized;
            vertexA3.Normal = normalDirection.normalized;
            vertexA4.Normal = normalDirection.normalized + Vector3.up;

            // Calculate column A tangent data
            Vector3 tangentDirection = Vector3.Cross(normalDirection, transitionDirection);

            // Set tangents
            vertexA1.Tangent = tangentDirection.normalized;
            vertexA2.Tangent = tangentDirection.normalized;
            vertexA3.Tangent = tangentDirection.normalized;
            vertexA4.Tangent = tangentDirection.normalized;

            // Column B
            Vector3 columnBAnchorPoint = (side.normalized * virtualBevelHalfWidth) + anchorPoint;

            Vertex vertexB1 = new Vertex(columnBAnchorPoint + offset1, baseIndex + 4);
            Vertex vertexB2 = new Vertex(columnBAnchorPoint + offset2, baseIndex + 5);
            Vertex vertexB3 = new Vertex(columnBAnchorPoint + offset3, baseIndex + 6);
            Vertex vertexB4 = new Vertex(columnBAnchorPoint + offset4, baseIndex + 7);

            // Calculate column B & C normals
            // Get left hand perpendicular vector to the above
            Vector3 normalDirectionBC = new Vector3(-side.normalized.z, 0f, side.normalized.x);

            // Set normals
            vertexB1.Normal = normalDirectionBC.normalized + Vector3.down;
            vertexB2.Normal = normalDirectionBC.normalized;
            vertexB3.Normal = normalDirectionBC.normalized;
            vertexB4.Normal = normalDirectionBC.normalized + Vector3.up;

            // Calculate column B & C tangent data
            Vector3 tangentDirectionBC = Vector3.Cross(normalDirection, side.normalized);

            // Set tangents
            vertexB1.Tangent = tangentDirectionBC.normalized;
            vertexB2.Tangent = tangentDirectionBC.normalized;
            vertexB3.Tangent = tangentDirectionBC.normalized;
            vertexB4.Tangent = tangentDirectionBC.normalized;

            // Column C
            Vector3 columnCAnchorPoint = (side - (side.normalized * virtualBevelHalfWidth)) + anchorPoint;

            Vertex vertexC1 = new Vertex(columnCAnchorPoint + offset1, baseIndex + 8);
            Vertex vertexC2 = new Vertex(columnCAnchorPoint + offset2, baseIndex + 9);
            Vertex vertexC3 = new Vertex(columnCAnchorPoint + offset3, baseIndex + 10);
            Vertex vertexC4 = new Vertex(columnCAnchorPoint + offset4, baseIndex + 11);

            // Set normals
            vertexC1.Normal = normalDirectionBC.normalized + Vector3.down;
            vertexC2.Normal = normalDirectionBC.normalized;
            vertexC3.Normal = normalDirectionBC.normalized;
            vertexC4.Normal = normalDirectionBC.normalized + Vector3.up;

            // Set tangents
            vertexC1.Tangent = tangentDirectionBC.normalized;
            vertexC2.Tangent = tangentDirectionBC.normalized;
            vertexC3.Tangent = tangentDirectionBC.normalized;
            vertexC4.Tangent = tangentDirectionBC.normalized;

            // Offset edge vertices just enough to avoid z-fighting
            float edgeDepthOffset = 0.0005f;

            vertexA1.Position -= vertexA2.Normal * edgeDepthOffset;
            vertexA2.Position -= vertexA2.Normal * edgeDepthOffset;
            vertexA3.Position -= vertexA3.Normal * edgeDepthOffset;
            vertexA4.Position -= vertexA2.Normal * edgeDepthOffset;

            vertexB1.Position -= vertexB1.Normal * edgeDepthOffset;
            vertexB4.Position -= vertexB4.Normal * edgeDepthOffset;

            vertexC1.Position -= vertexC1.Normal * edgeDepthOffset;
            vertexC4.Position -= vertexC4.Normal * edgeDepthOffset;

            // Add vertices to list
            vertices.Add(vertexA1);
            vertices.Add(vertexA2);
            vertices.Add(vertexA3);
            vertices.Add(vertexA4);

            vertices.Add(vertexB1);
            vertices.Add(vertexB2);
            vertices.Add(vertexB3);
            vertices.Add(vertexB4);

            vertices.Add(vertexC1);
            vertices.Add(vertexC2);
            vertices.Add(vertexC3);
            vertices.Add(vertexC4);
        }

        // Create border vertices
        for (int i = 0; i < points.Length; i++) {
            int baseIndex = i * 12;

            // Get base vertices to extend from
            Vertex baseVertexA = vertices[baseIndex % sideDataSize];
            Vertex baseVertexB = vertices[(baseIndex + 4) % sideDataSize];
            Vertex baseVertexC = vertices[(baseIndex + 8) % sideDataSize];
            Vertex baseVertexD = vertices[(baseIndex + 12) % sideDataSize];

            // Calculate extension points
            Vector3 innerPointA = baseVertexA.Position - ((baseVertexA.Normal - Vector3.down) * virtualBevelHalfWidth);
            Vector3 innerPointD = baseVertexD.Position - ((baseVertexD.Normal - Vector3.down) * virtualBevelHalfWidth);

            Vector3 innerSide = innerPointD - innerPointA;
            
            Vector3 innerPointB = innerPointA + (innerSide.normalized * virtualBevelHalfWidth);
            Vector3 innerPointC = innerPointA + (innerSide - (innerSide.normalized * virtualBevelHalfWidth));

            // Bottom border vertices
            // Calculate normal and tangent
            Vector3 bottomNormal = Vector3.down;
            Vector3 bottomTangent = Vector3.Cross(bottomNormal, innerSide.normalized);

            Vertex bottomInnerVertexA = new Vertex(innerPointA);
            Vertex bottomInnerVertexB = new Vertex(innerPointB);
            Vertex bottomInnerVertexC = new Vertex(innerPointC);

            bottomInnerVertexA.Normal = bottomNormal;
            bottomInnerVertexB.Normal = bottomNormal;
            bottomInnerVertexC.Normal = bottomNormal;

            bottomInnerVertexA.Tangent = bottomTangent;
            bottomInnerVertexB.Tangent = bottomTangent;
            bottomInnerVertexC.Tangent = bottomTangent;

            bottomInnerVertexA.Index = vertices.Count;
            vertices.Add(bottomInnerVertexA);
            bottomInnerVertexB.Index = vertices.Count;
            vertices.Add(bottomInnerVertexB);
            bottomInnerVertexC.Index = vertices.Count;
            vertices.Add(bottomInnerVertexC);

            bottomPolygonVertexChain.Add(bottomInnerVertexA);
            bottomPolygonVertexChain.Add(bottomInnerVertexB);
            bottomPolygonVertexChain.Add(bottomInnerVertexC);

            // Top border vertices
            // Calculate normal and tangent
            Vector3 topNormal = Vector3.up;
            Vector3 topTangent = Vector3.Cross(topNormal, innerSide.normalized);

            Vertex topInnerVertexA = new Vertex(innerPointA + (offset4 - offset1));
            Vertex topInnerVertexB = new Vertex(innerPointB + (offset4 - offset1));
            Vertex topInnerVertexC = new Vertex(innerPointC + (offset4 - offset1));

            topInnerVertexA.Normal = topNormal;
            topInnerVertexB.Normal = topNormal;
            topInnerVertexC.Normal = topNormal;

            topInnerVertexA.Tangent = topTangent;
            topInnerVertexB.Tangent = topTangent;
            topInnerVertexC.Tangent = topTangent;

            topInnerVertexA.Index = vertices.Count;
            vertices.Add(topInnerVertexA);
            topInnerVertexB.Index = vertices.Count;
            vertices.Add(topInnerVertexB);
            topInnerVertexC.Index = vertices.Count;
            vertices.Add(topInnerVertexC);

            // Set siblings
            bottomInnerVertexA.Sibling = topInnerVertexA;
            topInnerVertexA.Sibling = bottomInnerVertexA;

            bottomInnerVertexB.Sibling = topInnerVertexB;
            topInnerVertexB.Sibling = bottomInnerVertexB;

            bottomInnerVertexC.Sibling = topInnerVertexC;
            topInnerVertexC.Sibling = bottomInnerVertexC;
        }

        // Create triangles from vertices
        // Create side triangles
        for (int i = 0; i < points.Length; i++) {
            int baseIndex = i * sideSectionDataSize;

            for (int j = 0; j < 12; j += 4) {
                for (int k = 0; k < 3; k++) {
                    Vertex vertexSE = vertices[(baseIndex + k + j) % sideDataSize];
                    Vertex vertexSW = vertices[(baseIndex + k + j + 4) % sideDataSize];
                    Vertex vertexNW = vertices[(baseIndex + k + j + 5) % sideDataSize];
                    Vertex vertexNE = vertices[(baseIndex + k + j + 1) % sideDataSize];

                    triangles.Add(new Triangle(vertexSE, vertexSW, vertexNE));
                    triangles.Add(new Triangle(vertexSW, vertexNW, vertexNE));
                }
            }
        }

        // Create bottom and top border triangles
        for (int i = 0; i < points.Length; i++) {
            int baseIndex = i * sideSectionDataSize;

            for (int j = 0; j < 2; j++) {
                for (int k = 0; k < 3; k++) {
                    Vertex vertexSE = vertices[(baseIndex + (k * 4) + (j * 3)) % sideDataSize];
                    Vertex vertexSW = vertices[(baseIndex + (k * 4) + (j * 3) + 4) % sideDataSize];
                    Vertex vertexNW = vertices[sideDataSize + (((i * capBorderSectionDataSize) + ((k == 2 ? k + 4 : k + 1) + (j * 3))) % capBorderDataSize)];
                    Vertex vertexNE = vertices[sideDataSize + (((i * capBorderSectionDataSize) + ((k + (j * 3)))) % capBorderDataSize)];

                    if (j == 0) {
                        triangles.Add(new Triangle(vertexSW, vertexSE, vertexNE));
                        triangles.Add(new Triangle(vertexNW, vertexSW, vertexNE));
                    } else {
                        triangles.Add(new Triangle(vertexSE, vertexSW, vertexNE));
                        triangles.Add(new Triangle(vertexSW, vertexNW, vertexNE));
                    }
                }
            }
        }

        // Chain together border vertices for triangulation
        bool isPolygonConcave = false;

	    for (int i = 0; i < bottomPolygonVertexChain.Count; i++) {
            int nextPos = ClampListIndex(i + 1, bottomPolygonVertexChain.Count);
            int prevPos = ClampListIndex(i - 1, bottomPolygonVertexChain.Count);

            bottomPolygonVertexChain[i].PreviousVertex = bottomPolygonVertexChain[prevPos];
            bottomPolygonVertexChain[i].NextVertex = bottomPolygonVertexChain[nextPos];

            bottomPolygonVertexChain[i].Sibling.PreviousVertex = bottomPolygonVertexChain[prevPos].Sibling;
            bottomPolygonVertexChain[i].Sibling.NextVertex = bottomPolygonVertexChain[nextPos].Sibling;

            CheckAndSetIfReflexOrConvex(bottomPolygonVertexChain[i]);

            if (bottomPolygonVertexChain[i].IsReflex) {
                isPolygonConcave = true;
            }
        }

        // If polygon is concave, use simpler calculation
        if (!isPolygonConcave) {
            for (int i = 2; i < bottomPolygonVertexChain.Count; i++) {
                // Bottom
                Vertex a = bottomPolygonVertexChain[0];
                Vertex b = bottomPolygonVertexChain[i - 1];
                Vertex c = bottomPolygonVertexChain[i];

                AddTriangleToListIfValid(triangles, new Triangle(a, c, b));

                // Top
                a = bottomPolygonVertexChain[0].Sibling;
                b = bottomPolygonVertexChain[i - 1].Sibling;
                c = bottomPolygonVertexChain[i].Sibling;

                AddTriangleToListIfValid(triangles, new Triangle(a, b, c));
            }
        } else {
            // Build initial ear list
            for (int i = 0; i < bottomPolygonVertexChain.Count; i++) {
		        IsVertexEar(bottomPolygonVertexChain[i], bottomPolygonVertexChain, earVertices);
	        }

            // Build triangles
            while (bottomPolygonVertexChain.Count > 2 && earVertices.Count > 0) {
                // This means we have just one triangle left
                if (bottomPolygonVertexChain.Count == 3) {
                    // The final triangle
                    // Bottom
                    AddTriangleToListIfValid(triangles, new Triangle(bottomPolygonVertexChain[0], bottomPolygonVertexChain[0].PreviousVertex, bottomPolygonVertexChain[0].NextVertex));

                    // Top
                    AddTriangleToListIfValid(triangles, new Triangle(bottomPolygonVertexChain[0].Sibling, bottomPolygonVertexChain[0].NextVertex.Sibling, bottomPolygonVertexChain[0].PreviousVertex.Sibling));

                    break;
                }

                // Make a triangle of the first ear
                Vertex earVertex = earVertices[0];

                Vertex earVertexPrev = earVertex.PreviousVertex;
                Vertex earVertexNext = earVertex.NextVertex;

                // Bottom
                Triangle newBottomTriangle = new Triangle(earVertex, earVertexPrev, earVertexNext);
                AddTriangleToListIfValid(triangles, newBottomTriangle);

                // Top
                Triangle newTopTriangle = new Triangle(earVertex.Sibling, earVertexNext.Sibling, earVertexPrev.Sibling);
                AddTriangleToListIfValid(triangles, newTopTriangle);

                // Remove the vertex from the lists
                earVertices.Remove(earVertex);
                bottomPolygonVertexChain.Remove(earVertex);

                // Update the previous vertex and next vertex
                earVertexPrev.NextVertex = earVertexNext;
                earVertexNext.PreviousVertex = earVertexPrev;

                // ...see if we have found a new ear by investigating the two vertices that was part of the ear
                CheckAndSetIfReflexOrConvex(earVertexPrev);
                CheckAndSetIfReflexOrConvex(earVertexNext);

                earVertices.Remove(earVertexPrev);
                earVertices.Remove(earVertexNext);

                IsVertexEar(earVertexPrev, vertices, earVertices);
                IsVertexEar(earVertexNext, vertices, earVertices);
            }
        }
    }

    private static int ClampListIndex(int index, int listSize) {
        index = ((index % listSize) + listSize) % listSize;
        return index;
    }

    private static void CheckAndSetIfReflexOrConvex(Vertex v) {
        v.IsReflex = false;
        v.IsConvex = false;

        // This is a reflex vertex if its triangle is oriented clockwise
        Vector2 a = v.PreviousVertex.TopDownPosition;
        Vector2 b = v.TopDownPosition;
        Vector2 c = v.NextVertex.TopDownPosition;

        if (IsTriangleOrientedClockwise(a, b, c)) {
            v.IsConvex= true;
        } else {
            v.IsReflex = true;
        }
    }

    private static bool IsTriangleOrientedClockwise(Vector2 p1, Vector2 p2, Vector2 p3) {
        bool isClockWise = true;
        float determinant = p1.x * p2.y + p3.x * p1.y + p2.x * p3.y - p1.x * p3.y - p3.x * p2.y - p2.x * p1.y;

        if (determinant > 0f) {
            isClockWise = false;
        }

        return isClockWise;
    }

    private static void IsVertexEar(Vertex v, List<Vertex> vertices, List<Vertex> earVertices) {
        // A reflex vertex cant be an ear!
        if (v.IsReflex) {
            return;
        }

        // This triangle to check point in triangle
        Vector2 a = v.PreviousVertex.TopDownPosition;
        Vector2 b = v.TopDownPosition;
        Vector2 c = v.NextVertex.TopDownPosition;

        bool hasPointInside = false;

        for (int i = 0; i < vertices.Count; i++) {
            // We only need to check if a reflex vertex is inside of the triangle
            if (vertices[i].IsReflex) {
                Vector2 p = vertices[i].TopDownPosition;

                // This means inside and not on the hull
                if (IsPointInTriangle(a, b, c, p)) {
                    hasPointInside = true;
                    break;
                }
            }
        }

        if (!hasPointInside) {
            earVertices.Add(v);
        }
    }

    private static bool IsPointInTriangle(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p) {
        bool isWithinTriangle = false;

        // Based on Barycentric coordinates
        float denominator = ((p2.y - p3.y) * (p1.x - p3.x) + (p3.x - p2.x) * (p1.y - p3.y));

        float a = ((p2.y - p3.y) * (p.x - p3.x) + (p3.x - p2.x) * (p.y - p3.y)) / denominator;
        float b = ((p3.y - p1.y) * (p.x - p3.x) + (p1.x - p3.x) * (p.y - p3.y)) / denominator;
        float c = 1 - a - b;

        // The point is within the triangle
        if (a > 0f && a < 1f && b > 0f && b < 1f && c > 0f && c < 1f) {
            isWithinTriangle = true;
        }

        return isWithinTriangle;
    }
    
    // Used to not create triangles across points for 180 degree angles
    private static void AddTriangleToListIfValid(List<Triangle> list, Triangle triangle) {
        float triangleArea = CalculateTriangleArea(triangle.VertexA.TopDownPosition, triangle.VertexB.TopDownPosition, triangle.VertexC.TopDownPosition);

        if (!triangleArea.Equals(0f)) {
            list.Add(triangle);
        }
    }

    private static float CalculateTriangleArea(Vector2 p1, Vector2 p2, Vector2 p3) {
        return 0.5f * ((p1.x * (p2.y - p3.y)) + (p2.x * (p3.y - p1.y)) + (p3.x * (p1.y - p2.y)));
    }
}
