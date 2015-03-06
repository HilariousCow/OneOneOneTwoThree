using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class RibbonizeMesh {

    public static Mesh RibbonizeLineMesh(Mesh inMesh)
    {

        if (inMesh.GetTopology(0) != MeshTopology.LineStrip)
        {
            Debug.LogError("Sorry, wrong topology");
        }

        //replace mesh data by going through it and stuff.
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector4> tangents = new List<Vector4>();
        List<Vector2> uvs = new List<Vector2>();
        List<Vector2> uv2s = new List<Vector2>();
        List<Color> colors = new List<Color>();


        //step through line strip constructing quads

        bool isLoop = (inMesh.vertices[0] - inMesh.vertices[inMesh.vertexCount - 1]).magnitude < float.Epsilon;

        int vertCount = inMesh.vertexCount;
        for (int index = 0; index < vertCount; index++)
        {

            int indexNext = index == vertCount - 1 ? 0 : index + 1; //assumes loop
            int indexPrev = index == 0 ? vertCount - 1 : index - 1; //assumes loop

            Vector3 pos = inMesh.vertices[index];
            Vector3 posNext = inMesh.vertices[indexNext];
            Vector3 posPrev = inMesh.vertices[indexPrev];

            //Vector3 normal = inMesh.normals[index];


            Vector3 dirToNext = (posNext - pos).normalized;
            Vector3 dirFromPrev = (pos - posPrev).normalized;

            //replacement. This allows us to construct the tangent on the shader itself.
            //todo: somehow rule out insane corners
            //this only makes sense if you double the verts so that these are very separate
            //quads.
            //but that only makes any sense for a fixed perspective
          /*  if(Vector3.Dot(dirToNext, dirFromPrev) < -0.5f)
            {
                dirToNext = dirFromPrev;
            }*/

            //Vector4 tangent = Vector3.Lerp(dirToNext.normalized, dirFromPrev.normalized, 0.5f).normalized; //average direction
            Vector4 tangentButActuallyDirFromPrev = dirFromPrev;
            tangentButActuallyDirFromPrev.w = 1.0f;//side

            float fraction = (float)index / (float)(vertCount - 1);
            Vector2 uv = Vector2.up * fraction;
            Vector2 uv2 = uv;
            Color col = inMesh.colors[index];

            //create 4 verts for every line segment
            //A - left

            vertices.Add(pos);
            normals.Add(dirToNext);
            tangents.Add(tangentButActuallyDirFromPrev);
            uvs.Add(uv);
            uv2s.Add(uv2);
            colors.Add(col);

            //B - right
            //flip tangent 
            Vector4 tangentButActuallyDirFromPrevFlipped = tangentButActuallyDirFromPrev;
            tangentButActuallyDirFromPrevFlipped.w = -1f;//side


            vertices.Add(pos);
            normals.Add(dirToNext);
            tangents.Add(tangentButActuallyDirFromPrevFlipped);
            uvs.Add(uv + Vector2.right);
            uv2s.Add(uv2 + Vector2.right);
            colors.Add(col);

        }

        //go through every pair of indices (marking a segment)
        List<int> indices = new List<int>();
        for (int i = 0; i < vertices.Count - 2; i += 2)
        {


            //for every 2 verts, we are going to add 1 quad
            indices.Add(i);
            indices.Add(i + 1);

            indices.Add(i + 3);
            indices.Add(i + 2);
        }

        if (isLoop)//todo: detecting that it's a loop, but the final quad doesn't seem to be there?
        {
            //   Debug.Log("is loop. Adding extra quad to bridge");
            indices.Add(vertices.Count - 2);
            indices.Add(vertices.Count - 1);

            indices.Add(1);
            indices.Add(0);
        }

        Mesh mesh = new Mesh();
        mesh.name = inMesh.name;


        mesh.vertices = vertices.ToArray();
        mesh.normals = normals.ToArray();
        mesh.tangents = tangents.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.uv2 = uv2s.ToArray();
        mesh.colors = colors.ToArray();
        //mesh.subMeshCount = 1;


        mesh.SetIndices(indices.ToArray(), MeshTopology.Quads, 0);
        mesh.RecalculateBounds();
        //also todo: shaders to unpack in vert()
        return mesh;
    }
}
