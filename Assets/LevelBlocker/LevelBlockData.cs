using UnityEngine;
using System.Collections.Generic;

public class LevelBlockData
{
    public List<Vertex> vertices;
    public List<Triangle> triangles;

    public Vector2 xBounds = new Vector2(-2, 2);
    public Vector2 zBounds = new Vector2(-2, 2);

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

        // Precalculate vertex side point height offsets
        Vector3 offset1 = Vector3.up * bottomHeight;
        Vector3 offset2 = (Vector3.up * bottomHeight) + (Vector3.up * virtualBevelHalfWidth);
        Vector3 offset3 = (Vector3.up * topHeight) + (Vector3.down * virtualBevelHalfWidth);
        Vector3 offset4 = Vector3.up * topHeight;

        // Create main side vertexes
        for (int i = 0; i < points.Length; i++) {
            int baseIndex = i * 12;

            Vector3 anchorPoint = points[i];
            Vector3 previousPoint = points[(i - 1 == -1 ? points.Length - 1 : i - 1)];
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

            // Update bounds based on point
            if (points[i].x < xBounds.x) xBounds.x = points[i].x;
            if (points[i].x > xBounds.y) xBounds.y = points[i].x;
            if (points[i].z < zBounds.x) zBounds.x = points[i].z;
            if (points[i].z > zBounds.y) zBounds.y = points[i].z;
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
            Vector3 topNormal = Vector3.down;
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
    }
}
