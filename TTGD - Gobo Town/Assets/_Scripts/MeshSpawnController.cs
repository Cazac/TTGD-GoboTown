using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshSpawnController : MonoBehaviour
{
    ////////////////////////////////


    public int hexCount;


    public GameObject emptyHex_Prefab;

    [Header("Hex Sizes")]
    public const float outerRadius = 0.1f;

    public const float innerRadius = outerRadius * 0.866025404f;

    public static Vector3[] corners = {
        new Vector3(0f, 0f, outerRadius),
        new Vector3(innerRadius, 0f, 0.5f * outerRadius),
        new Vector3(innerRadius, 0f, -0.5f * outerRadius),
        new Vector3(0f, 0f, -outerRadius),
        new Vector3(-innerRadius, 0f, -0.5f * outerRadius),
        new Vector3(-innerRadius, 0f, 0.5f * outerRadius),
        new Vector3(0f, 0f, outerRadius)
    };

    /////////////////////////////////////////////////////////////////

    private void Start()
    {
        for (int i = 0; i < hexCount; i++)
        {
            SpawnHex_Hard();
            SpawnHex_Soft();
        }
    }

    /////////////////////////////////////////////////////////////////

    private void HexMap_Spawn()
    {








    }

    /////////////////////////////////////////////////////////////////

    public void SpawnHex_Hard()
    {
        GameObject newHex = Instantiate(emptyHex_Prefab);
        newHex.name = "Hex Mesh (Hard)";

        MeshFilter meshFilter = newHex.GetComponent<MeshFilter>();
        Mesh hexMesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        hexMesh.Clear();
        vertices.Clear();
        triangles.Clear();

        GenerateMesh_Top(vertices, triangles);
        GenerateMesh_Side(vertices, triangles);

        hexMesh.vertices = vertices.ToArray();
        hexMesh.triangles = triangles.ToArray();
        hexMesh.RecalculateNormals();


        meshFilter.mesh = hexMesh;
    }

    public void SpawnHex_Soft()
    {
        GameObject newHex = Instantiate(emptyHex_Prefab);
        newHex.name = "Hex Mesh (Soft)";

        MeshFilter meshFilter = newHex.GetComponent<MeshFilter>();
        Mesh hexMesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        hexMesh.Clear();
        vertices.Clear();
        triangles.Clear();

        Vector3 center = new Vector3(1f, 0.1f, 0f);
        GenerateTriangle_All(center, vertices, triangles);

        hexMesh.vertices = vertices.ToArray();
        hexMesh.triangles = triangles.ToArray();
        hexMesh.RecalculateNormals();


        meshFilter.mesh = hexMesh;
    }

    /////////////////////////////////////////////////////////////////

    private void GenerateMesh_Side(List<Vector3> vertices, List<int> triangles)
    {
        Vector3 height = new Vector3(0f, -0.1f, 0f);
        Vector3 center = new Vector3(0f, 0.1f, 0f);


        for (int i = 0; i < 6; i++)
        {
            GenerateTriangle(
                center + corners[i] + height,
                center + corners[i + 1],
                center + corners[i],
                vertices,
                triangles
            );
        }



        for (int i = 0; i < 6; i++)
        {
            GenerateTriangle(
                center + corners[i] + height,
                center + corners[i + 1] + height,
                center + corners[i + 1],

                vertices,
                triangles
            );
        }
    }

    /////////////////////////////////////////////////////////////////

    private void GenerateMesh_Top(List<Vector3> vertices, List<int> triangles)
    {
        //Vector3 center = cell.transform.localPosition;
        for (int i = 0; i < 6; i++)
        {
            Vector3 center = new Vector3(0f, 0.1f, 0f);

            GenerateTriangle(
                center,
                center + corners[i],
                center + corners[i + 1],
                vertices,
                triangles
            );
            
        }
    }

    private void GenerateTriangle(Vector3 v1, Vector3 v2, Vector3 v3, List<Vector3> vertices, List<int> triangles)
    {
        int vertexIndex = vertices.Count;
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
    }

    private void GenerateTriangle_All(Vector3 center, List<Vector3> vertices, List<int> triangles)
    {
        
        vertices.Add(center);
        vertices.Add(center + corners[0]);
        vertices.Add(center + corners[1]);
        vertices.Add(center + corners[2]);
        vertices.Add(center + corners[3]);
        vertices.Add(center + corners[4]);
        vertices.Add(center + corners[5]);

        triangles.Add(0);
        triangles.Add(1);
        triangles.Add(2);

        triangles.Add(0);
        triangles.Add(2);
        triangles.Add(3);

        triangles.Add(0);
        triangles.Add(3);
        triangles.Add(4);

        triangles.Add(0);
        triangles.Add(4);
        triangles.Add(5);

        triangles.Add(0);
        triangles.Add(5);
        triangles.Add(6);

        triangles.Add(0);
        triangles.Add(6);
        triangles.Add(1);



        Vector3 height = new Vector3(0f, 0.1f, 0f);

        vertices.Add(vertices[1] - height);
        vertices.Add(vertices[2] - height);
        vertices.Add(vertices[3] - height);
        vertices.Add(vertices[4] - height);
        vertices.Add(vertices[5] - height);
        vertices.Add(vertices[6] - height);



        triangles.Add(2);
        triangles.Add(1);
        triangles.Add(7);

        triangles.Add(3);
        triangles.Add(2);
        triangles.Add(8);

        triangles.Add(4);
        triangles.Add(3);
        triangles.Add(9);

        triangles.Add(5);
        triangles.Add(4);
        triangles.Add(10);

        triangles.Add(6);
        triangles.Add(5);
        triangles.Add(11);

        triangles.Add(1);
        triangles.Add(6);
        triangles.Add(12);






        //


 
        triangles.Add(7);
        triangles.Add(8);
        triangles.Add(2);

        triangles.Add(8);
        triangles.Add(9);
        triangles.Add(3);

        triangles.Add(9);
        triangles.Add(10);
        triangles.Add(4);

        triangles.Add(10);
        triangles.Add(11);
        triangles.Add(5);

        triangles.Add(11);
        triangles.Add(12);
        triangles.Add(6);

        triangles.Add(12);
        triangles.Add(7);
        triangles.Add(1);
    }

    /////////////////////////////////////////////////////////////////



}