using UnityEngine;

public class LevelBlockData
{
    private Quad[] quads;
    private Triangle[] triangles;

    public LevelBlockData(Vector3[] points) {
        int nQuadsInSide = 9;
        int nQuadsOnCap = 3;
        int nQuadsInBorderSection = nQuadsInSide + (nQuadsOnCap * 2);
        int nQuadsInBlock = nQuadsInBorderSection * points.Length;

        int nTrianglesInPolygon = points.Length - 2;
        int nPolygonsInBlock = nTrianglesInPolygon * 2;

        quads = new Quad[nQuadsInBlock];
        triangles = new Triangle[nPolygonsInBlock];
    }
}
