using UnityEngine;

public class Triangle
{
    private Vertex VertexA { get; set; }
    private Vertex VertexB { get; set; }
    private Vertex VertexC { get; set; }

    public Triangle(Vector3 positionA, Vector3 positionB, Vector3 positionC) {
        VertexA = new Vertex(positionA);
        VertexB = new Vertex(positionB);
        VertexC = new Vertex(positionC);
    }

    public Triangle(Vertex vertexA, Vertex vertexB, Vertex vertexC) {
        VertexA = vertexA;
        VertexB = vertexB;
        VertexC = vertexC;
    }

    public void ChangeOrientation() {
        Vertex temp = VertexA;

        VertexA = VertexB;
        VertexB = temp;
    }
}
