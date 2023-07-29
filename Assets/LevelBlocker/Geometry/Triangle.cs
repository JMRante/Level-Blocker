using UnityEngine;

public class Triangle
{
    public Vertex VertexA { get; set; }
    public Vertex VertexB { get; set; }
    public Vertex VertexC { get; set; }

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
