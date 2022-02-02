using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CylinderGenerate
{
    [Range(10, 20)]
    public static int Iterations = 10;
    [Min(0)]
    public static int Height;
    [Min(0)]
    public static int Radius;
    [Min(0)]
    public static float Gap = 0.5f;

    private static Mesh m_Mesh;
    private static MeshRenderer m_MeshRenderer;
    private static MeshFilter m_MeshFilter;
    private static GameObject m_Primitive;
    private static int m_Num;
    private static Vector3[] m_Vertices;
    private static int[] m_Tris;
    private static int[] m_FinalTris;
    private static int[] m_FirstPlane;
    public static void CreateCylinder(Transform parent, int radius, int iterations, int height, int gap)
    {
        m_Primitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
        m_Primitive.transform.position = parent.position;

        m_Mesh = new Mesh();
        m_MeshFilter = m_Primitive.GetComponent<MeshFilter>();
        m_MeshRenderer = m_Primitive.GetComponent<MeshRenderer>();

        MakingVertices(Radius, Iterations, Height, Gap);
    }

    static void MakingVertices(int radius, int iterations, int height, float gap)
    {
        float x;
        float y;
        float z = 0;
        int i;
        int p = 0;
        float angle;

        m_Vertices = new Vector3[(iterations * height) + 2];
        int tempo = 0;
        m_Vertices[m_Vertices.Length - 2] = Vector3.zero;

        while (p < height)
        {
            i = 0;
            while (i < iterations)
            {
                angle = (i * 1.0f) / iterations * Mathf.PI * 2;
                x = Mathf.Sin(angle) * radius;
                y = Mathf.Cos(angle) * radius;
                m_Vertices[tempo] = new Vector3(x, y, z);
                i++;
                m_Num++;
                tempo += 1;
            }
            z += gap;
            p++;
        }


        m_Vertices[m_Vertices.Length - 1] = new Vector3(0, 0, m_Vertices[m_Vertices.Length - 3].z);
        Debug.Log("Vertices: " + m_Num);
        m_Mesh.vertices = m_Vertices;
        MakeNormals();
    }
    static void MakeNormals()
    {
        int i = 0;
        Vector3[] normals = new Vector3[m_Num + 2];
        while (i < m_Num)
        {
            normals[i] = Vector3.forward;
            i++;
        }
        m_Mesh.normals = normals;

        MakeTriangles();
    }
    static void MakeTriangles()
    {
        int i = 0;
        m_Tris = new int[((3 * ((int)Height - 1) * Iterations) * 2) + 3];
        while (i < (Height - 1) * Iterations)
        {
            m_Tris[i * 3] = i;
            if ((i + 1) % Iterations == 0)
            {
                m_Tris[i * 3 + 1] = 1 + i - Iterations;
            }
            else
            {
                m_Tris[i * 3 + 1] = 1 + i;
            }
            m_Tris[i * 3 + 2] = Iterations + i;
            i++;
        }
        int newTrisIdx = -1;

        for (int u = (m_Tris.Length - 3) / 2; u < m_Tris.Length - 6; u += 3)
        {
            //mesh.RecalculateTangents();
            if ((newTrisIdx + 2) % Iterations == 0)
            {
                m_Tris[u] = newTrisIdx + Iterations * 2 + 1;
            }
            else
                m_Tris[u] = newTrisIdx + Iterations + 1;

            m_Tris[u + 1] = newTrisIdx + 2;
            m_Tris[u + 2] = newTrisIdx + Iterations + 2;
            newTrisIdx += 1;
        }
        m_Tris[m_Tris.Length - 3] = 0;
        m_Tris[m_Tris.Length - 2] = (Iterations * 2) - 1;
        m_Tris[m_Tris.Length - 1] = Iterations;

        m_FirstPlane = new int[(Iterations * 3) * 2];
        int felmnt = 0;
        for (int h = 0; h < m_FirstPlane.Length / 2; h += 3)
        {

            m_FirstPlane[h] = felmnt;

            if (felmnt + 1 != Iterations)
                m_FirstPlane[h + 1] = felmnt + 1;
            else
                m_FirstPlane[h + 1] = 0;
            m_FirstPlane[h + 2] = m_Vertices.Length - 2;
            felmnt += 1;
        }

        felmnt = Iterations * (Height - 1);
        for (int h = m_FirstPlane.Length / 2; h < m_FirstPlane.Length; h += 3)
        {

            m_FirstPlane[h] = felmnt;

            if (felmnt + 1 != Iterations * (Height - 1))
                m_FirstPlane[h + 1] = felmnt + 1;
            else
                m_FirstPlane[h + 1] = Iterations * (Height - 1);
            m_FirstPlane[h + 2] = m_Vertices.Length - 1;
            felmnt += 1;
        }

        m_FirstPlane[m_FirstPlane.Length - 3] = Iterations * (Height - 1);
        m_FirstPlane[m_FirstPlane.Length - 2] = m_Vertices.Length - 3;
        m_FirstPlane[m_FirstPlane.Length - 1] = m_Vertices.Length - 1;

        m_FinalTris = new int[m_Tris.Length + m_FirstPlane.Length];

        int k = 0, l = 0;
        for (k = 0, l = 0; k < m_Tris.Length; k++)
        {
            m_FinalTris[l++] = m_Tris[k];
        }
        for (k = 0; k < m_FirstPlane.Length; k++)
        {
            m_FinalTris[l++] = m_FirstPlane[k];
        }

        m_Mesh.triangles = m_FinalTris;
        m_Mesh.Optimize();
        m_Mesh.RecalculateNormals();
        m_MeshFilter.mesh = m_Mesh;
        m_MeshRenderer.enabled = false;

        MeshCollider m_MeshCollider = m_Primitive.AddComponent<MeshCollider>();
        m_MeshCollider.sharedMesh = m_Mesh;
        m_MeshCollider.convex = true;
        m_MeshCollider.isTrigger = true;
    }
}