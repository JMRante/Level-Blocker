using UnityEngine;

public class Quad
{
    private Triangle triangleA;
    private Triangle triangleB;

    public Quad(Vector3 cornerA, Vector3 cornerB, Vector3 normal) {
        // Vector3 minCorner = new Vector3(Mathf.Min(cornerA.x, cornerB.x), Mathf.Min(cornerA.y, cornerB.y), Mathf.Min(cornerA.z, cornerB.z));
        // Vector3 maxCorner = new Vector3(Mathf.Max(cornerA.x, cornerB.x), Mathf.Max(cornerA.y, cornerB.y), Mathf.Max(cornerA.z, cornerB.z));

        // Vector3 quadCross = maxCorner - minCorner;

        // Vector3 sideCornerA = Vector3.Cross(quadCross, normal).normalized * quadCross.magnitude;
        // Vector3 sideCornerB = Vector3.Cross(normal, quadCross).normalized * quadCross.magnitude;

        // Vector3 point1 = minCorner;
        // Vector3 point2 = sideCornerA;
        // Vector3 point3 = sideCornerB;
        // Vector3 point4 = maxCorner;

        // triangleA = new Triangle(point1, point3, point2);
        // triangleB = new Triangle(point2, point4, point3);
    }
}
