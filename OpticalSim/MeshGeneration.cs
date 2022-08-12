using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LightTK;
using static LightTK.LTK;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MeshGeneration : MonoBehaviour
{
    List<Vector3> vertices = new List<Vector3>();
    List<Vector3> normals = new List<Vector3>();
    List<Vector2> uvs = new List<Vector2>();

    List<int> triangles = new List<int>();

    private void Start() {
        GenerateMesh();
    }

    public void GenerateMesh() {
        Mesh m = new Mesh();
        
        Curve c = new Hyperboloid(0f, 0f, 5f);
        c.parameters.h = -5;

        LightRayHit[] hits = new LightRayHit[2];

        float step = 0.05f;
        int gridSize = 40;

        for(int j = gridSize / 2 - gridSize; j < gridSize / 2; j++) {
            for(int i = gridSize / 2 - gridSize; i < gridSize / 2; i++) {
                float x = i * step;
                float y = j * step;
                int hitCount = GetRelativeIntersection(new Vector3(x, y, 0), Vector3.forward, c, hits);

                if (hitCount == 1)
                {
                    vertices.Add(hits[0].point);
                    normals.Add(hits[0].normal);
                    uvs.Add(new Vector2((i + gridSize - gridSize / 2)/gridSize, j + gridSize - gridSize / 2) / gridSize);
                }
            }
        }

        for (int i = 0; i < gridSize * (gridSize - 1) - 1; i++)
        {
            if ((i+1) % gridSize == 0) continue;

            triangles.AddRange(new List<int>() {
                i, i + gridSize, i + 1 + gridSize,
                i + 1 + gridSize, i + 1, i,

                //i + 0, i + gridSize + 1, i + 1,
                //i + 1, i + gridSize + 1, i + gridSize + 2
            });
        }

        m.SetVertices(vertices);
        m.SetNormals(normals);
        m.SetUVs(0, uvs);
        m.SetTriangles(triangles, 0);

        GetComponent<MeshFilter>().mesh = m;
    }

    //private void OnDrawGizmos() {
    //    if (vertices == null) return;

    //    for(int i = 0; i < vertices.Count; i++)
    //    {
    //        Gizmos.DrawSphere(vertices[i], 0.02f);
    //    }
    //}
}
