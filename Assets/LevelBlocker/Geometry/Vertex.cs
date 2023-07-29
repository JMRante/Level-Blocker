using UnityEngine;

public class Vertex
{
    public Vector3 Position
    {
        get { return position; }
        set { 
            position = value;
            topDownPosition = new Vector2(position.x, position.z);
        }
    }
    private Vector3 position;

    public Vector2 TopDownPosition { 
        get => topDownPosition; 
    }
    private Vector2 topDownPosition;

    public Vector3 Normal { get; set; }
    public Vector3 Tangent { get; set; }

    public Triangle Triangle { get; set; }

    public Vertex PreviousVertex { get; set; }
    public Vertex NextVertex { get; set; }

    public bool IsReflex { get; set; }
    public bool IsConvex { get; set; }
    public bool IsEar { get; set; }

    public int Index { get; set; }

    public Vertex(Vector3 position) {
        Position = position;
    }

    public Vertex(Vector3 position, int index) {
        Position = position;
        Index = index;
    }
}
