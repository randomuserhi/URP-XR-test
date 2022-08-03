using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BetterLineRenderer : MonoBehaviour
{
    public static List<Vector3[]> positions { get; set; } = new List<Vector3[]>();
    public Material mat;

    private void OnPostRender()
    {
        DrawLines();
    }

    private void OnDrawGizmos()
    {
        DrawLines();
    }

    private void DrawLines()
    {
        if (positions.Count == 0)
            return;

        for (int j = 0; j < positions.Count; j++)
        {
            for (int i = 0; i < positions[j].Length - 1; ++i)
            {
                GL.Begin(GL.LINES);
                mat.SetPass(0);
                GL.Color(mat.color);
                GL.Vertex(positions[j][i]);
                GL.Vertex(positions[j][i + 1]);
                GL.End();
            }
        }
    }
}
