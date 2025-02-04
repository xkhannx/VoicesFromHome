using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class RoadMeshCreator:MonoBehaviour
{
    public float tiling = 1;
    public void UpdateRoadMesh(Vector4[] points, Quaternion[] rots, float spacing, float roadWidth)
    {
        GetComponent<MeshFilter>().mesh = CreateRoadMesh(points, rots, roadWidth);

        //int textureRepeat = Mathf.RoundToInt(tiling * points.Length * spacing * 0.05f);
        //GetComponent<MeshRenderer>().sharedMaterial.SetTextureScale("_BaseMap", new Vector2(1, textureRepeat));
    }

    Mesh CreateRoadMesh(Vector4[] points, Quaternion[] rots, float roadWidth)
    {
        Vector3[] verts = new Vector3[points.Length * 2];
        Vector2[] uvs = new Vector2[points.Length * 2];
        int[] tris = new int[2 * (points.Length - 1) * 3];

        int vertInd = 0;
        int triInd = 0;

        for (int i = 0; i < points.Length; i++)
        {
            Vector3 curPoint = new Vector3(points[i].x, points[i].y, points[i].z);
            Vector3 left = rots[i] * Vector3.left;

            verts[vertInd] = curPoint + left * roadWidth / 2;
            verts[vertInd + 1] = curPoint - left * roadWidth / 2;

            float completionPercent = i / (float)(points.Length - 1);
            uvs[vertInd] = new Vector2(0, completionPercent);
            uvs[vertInd + 1] = new Vector2(1, completionPercent);

            if (i < points.Length - 1)
            {
                tris[triInd]     = vertInd;
                tris[triInd + 1] = vertInd + 2;
                tris[triInd + 2] = vertInd + 1;

                tris[triInd + 3] = vertInd + 1;
                tris[triInd + 4] = vertInd + 2;
                tris[triInd + 5] = vertInd + 3;
            }

            vertInd += 2;
            triInd += 6;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.uv = uvs;

        return mesh;
    }
}
