using System.Collections.Generic;
using UnityEngine;

/// Генерация снежинки Коха: N-угольник + параметрический угол выступа.
public static class KochCurveGenerator
{
    /// Строит снежинку. length — диаметр описанной окружности базового многоугольника.
    public static List<Vector2> GenerateSnowflake(int sides, float length, int iterations, float angleDegrees)
    {
        if (sides < 3) sides = 3;
        if (iterations < 0) iterations = 0;
        angleDegrees = Mathf.Clamp(angleDegrees, 1f, 179f);

        var polygon = CreatePolygon(sides, length);
        var output = new List<Vector2>();

        for (int i = 0; i < polygon.Count; i++)
        {
            Vector2 start = polygon[i];
            Vector2 end = polygon[(i + 1) % polygon.Count];
            Subdivide(start, end, iterations, angleDegrees, output);
        }
        return output;
    }

    private static List<Vector2> CreatePolygon(int sides, float length)
    {
        var pts = new List<Vector2>(sides);
        float step = Mathf.PI * 2f / sides;
        for (int i = 0; i < sides; i++)
        {
            float a = i * step - Mathf.PI * 0.5f;   // как в Godot-версии
            pts.Add(new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * (length * 0.5f));
        }
        return pts;
    }

    private static void Subdivide(Vector2 start, Vector2 end, int depth, float angleDeg, List<Vector2> output)
    {
        if (depth == 0)
        {
            if (output.Count == 0) output.Add(start);
            output.Add(end);
            return;
        }

        Vector2 diff = end - start;
        Vector2 a = start + diff / 3f;
        Vector2 b = start + diff * (2f / 3f);

        Vector2 dir = diff.normalized;
        float segLen = diff.magnitude / 3f;
        // Внешняя нормаль (для CCW-многоугольника) — пик идёт наружу.
        Vector2 perp = new Vector2(dir.y, -dir.x);

        float tanVal = Mathf.Tan(angleDeg * Mathf.Deg2Rad);
        if (float.IsNaN(tanVal) || float.IsInfinity(tanVal)) tanVal = 20f;
        tanVal = Mathf.Clamp(tanVal, -20f, 20f);   // страховка от 90°

        float height = tanVal * segLen * 0.5f;
        Vector2 mid = (a + b) * 0.5f;
        Vector2 peak = mid + perp * height;

        Subdivide(start, a, depth - 1, angleDeg, output);
        Subdivide(a, peak, depth - 1, angleDeg, output);
        Subdivide(peak, b, depth - 1, angleDeg, output);
        Subdivide(b, end, depth - 1, angleDeg, output);
    }
}