using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class KochCurveRenderer : MonoBehaviour
{
    public Color color = new Color(0.35f, 0.85f, 1f);

    private LineRenderer _lr;
    private Vector3[] _buffer;

    private LineRenderer Lr
    {
        get
        {
            if (_lr == null)
            {
                _lr = GetComponent<LineRenderer>();
                _lr.useWorldSpace = false;
                _lr.numCapVertices = 4;
                _lr.numCornerVertices = 4;
            }
            return _lr;
        }
    }

    public void Render(List<Vector2> points, float thickness)
    {
        if (points == null || points.Count < 2)
        {
            Lr.positionCount = 0;
            return;
        }

        if (_buffer == null || _buffer.Length != points.Count)
            _buffer = new Vector3[points.Count];

        for (int i = 0; i < points.Count; i++)
            _buffer[i] = new Vector3(points[i].x, points[i].y, 0f);

        Lr.positionCount = points.Count;
        Lr.SetPositions(_buffer);
        Lr.startWidth = thickness;
        Lr.endWidth = thickness;
        Lr.startColor = color;
        Lr.endColor = color;
    }
}