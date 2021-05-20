using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Random = UnityEngine.Random;

public class HexCell : MonoBehaviour
{
    ////////////////////////////////

    public enum HexDirection
    {
        N, NE, SE, S, SW, NW
    }

    ////////////////////////////////

    public static Vector3[] hexMeshCorners = 
    {
        new Vector3(0f, 0f, HexSpawnController.outerRadius),
        new Vector3(HexSpawnController.innerRadius, 0f, 0.5f * HexSpawnController.outerRadius),
        new Vector3(HexSpawnController.innerRadius, 0f, -0.5f * HexSpawnController.outerRadius),
        new Vector3(0f, 0f, -HexSpawnController.outerRadius),
        new Vector3(-HexSpawnController.innerRadius, 0f, -0.5f * HexSpawnController.outerRadius),
        new Vector3(-HexSpawnController.innerRadius, 0f, 0.5f * HexSpawnController.outerRadius),
        new Vector3(0f, 0f, HexSpawnController.outerRadius)
    };

    ////////////////////////////////

    [SerializeField]
    public HexCell[] neighbors;

    [Header("Visable Hex Coords")]
    public HexCoords hexCoords;

    [Header("Debug Visual Labels")]
    public TextMeshProUGUI hexLabel_Text;

    [Header("Hex Object Information")]
    public GameObject hexObject_GO;
    public MeshFilter hexObject_MeshFilter;
    public MeshRenderer hexObject_MeshRenderer;
    public MeshCollider hexObject_MeshCollider;
    public GameObject IDCanvas;

    [Header("Hex Color Options")]
    public Color colorActive;



    public int hexCellMatID;

    private Color[] colors_Arr;

    /////////////////////////////////////////////////////////////////

    public HexCell GetNeighbor(HexDirection direction)
    {
        return neighbors[(int)direction];
    }

    public void SetNeighbor(HexDirection direction, HexCell cell)
    {
        neighbors[(int)direction] = cell;
        cell.neighbors[(int)GetOppositeDirection(direction)] = this;
    }

    public HexDirection GetOppositeDirection(HexDirection direction)
    {
        int directionValue = (int)direction;

        if (directionValue < 3)
        {
            return direction + 3;
        }
        else
        {
            return direction - 3;
        }
    }

    /////////////////////////////////////////////////////////////////

    public void SetLabel(int x, int z)
    {
        hexCoords = HexCoords.GenerateCoords(x, z);
        string hexID = (x.ToString() + "/" + z.ToString());

        hexLabel_Text.text = hexID;
        gameObject.name = hexID;
    }

    /////////////////////////////////////////////////////////////////

    public void ClickCell()
    {
        //Update The color to a Clicked Version
        //UpdateCellColor(colorClicked);

        //Turn On / Off Canvas Location
        if (IDCanvas.activeSelf == true)
        {
            IDCanvas.SetActive(false);
        }
        else
        {
            IDCanvas.SetActive(true);
        }
    }

    /////////////////////////////////////////////////////////////////

    public void GenerateCellColor(Gradient colorRangeGradient)
    {
        //Set Random Value From Gradeint
        colorActive = colorRangeGradient.Evaluate(Random.Range(0, 1f));
    }

    public void UpdateMaterial(int matID, Material hexCellColorMaterial_PlainsTextured)
    {
        hexCellMatID = matID;
        hexObject_MeshRenderer.material = hexCellColorMaterial_PlainsTextured;
    }

    public void UpdateCellColor(Color newColor)
    {
        //Get Mesh and Create Array
        Mesh mesh = hexObject_MeshFilter.mesh;
        colors_Arr = new Color[mesh.vertices.Length];

        //Loop All Values TO Single Color
        for (int i = 0; i < colors_Arr.Length; i++)
        {
            //This is mutipled by the current texture Color info 
            colors_Arr[i] = newColor;
        }

        //Set New Color Array To Mesh
        mesh.colors = colors_Arr;

        // Not Needed ??
        mesh.RecalculateNormals();
    }

    /////////////////////////////////////////////////////////////////

    public void GenerateHeight_Random()
    {
        //Randomzie Height
        float height = Random.Range(0, 0.015f);
        SetCellHeight(height);
    }

    public void GenerateHeight_Linked()
    {

    }

    public void SetCellHeight(float height)
    {
        gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + height, gameObject.transform.position.z);
    }

    /////////////////////////////////////////////////////////////////

    public void GenerateHexMesh_Hard()
    {
        //Create New Mesh
        List<Vector3> verts_List = new List<Vector3>();
        List<int> tris_List = new List<int>();
   
        //Generate Verts / Tris
        GenerateMesh_Top(verts_List, tris_List);
        GenerateMesh_Side(verts_List, tris_List);

        //Generate UVs
        Vector2[] uvs_Arr = new Vector2[verts_List.Count];
        GenerateMesh_UVs(verts_List, tris_List, uvs_Arr);

        //Refresh The Mesh
        GenerateMesh_Final(verts_List, tris_List, uvs_Arr);
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
                center + hexMeshCorners[i],
                center + hexMeshCorners[i + 1],
                vertices,
                triangles
            );

        }
    }

    private void GenerateMesh_Side(List<Vector3> vertices, List<int> triangles)
    {
        Vector3 height = new Vector3(0f, -0.5f, 0f);
        Vector3 center = new Vector3(0f, 0.1f, 0f);


        for (int i = 0; i < 6; i++)
        {
            GenerateTriangle(
                center + hexMeshCorners[i] + height,
                center + hexMeshCorners[i + 1],
                center + hexMeshCorners[i],
                vertices,
                triangles
            );
        }



        for (int i = 0; i < 6; i++)
        {
            GenerateTriangle(
                center + hexMeshCorners[i] + height,
                center + hexMeshCorners[i + 1] + height,
                center + hexMeshCorners[i + 1],

                vertices,
                triangles
            );
        }
    }

    private void GenerateMesh_UVs(List<Vector3> verts_List, List<int> tris_List, Vector2[] uvs_Arr)
    {
        float scaleFactor = 0.1f;

        // Iterate over each face (here assuming triangles)
        for (int i = 0; i < tris_List.Count; i += 3)
        {
            // Get the three vertices bounding this triangle.
            Vector3 v1 = verts_List[tris_List[i]];
            Vector3 v2 = verts_List[tris_List[i + 1]];
            Vector3 v3 = verts_List[tris_List[i + 2]];


            // Compute a vector perpendicular to the face.
            Vector3 normal = Vector3.Cross(v3 - v1, v2 - v1);

            // Form a rotation that points the z+ axis in this perpendicular direction.
            // Multiplying by the inverse will flatten the triangle into an xy plane.
            Quaternion rotation = Quaternion.Inverse(Quaternion.LookRotation(normal));

            // Assign the uvs, applying a scale factor to control the texture tiling.
            uvs_Arr[tris_List[i]] = (Vector2)(rotation * v1) * scaleFactor;
            uvs_Arr[tris_List[i + 1]] = (Vector2)(rotation * v2) * scaleFactor;
            uvs_Arr[tris_List[i + 2]] = (Vector2)(rotation * v3) * scaleFactor;
        }

    }

    private void GenerateMesh_Final(List<Vector3> verts_List, List<int> tris_List, Vector2[] uvs_Arr)
    {
        //Create New Mesh
        Mesh hexMesh = new Mesh();

        //Set Mesh Info
        hexMesh.vertices = verts_List.ToArray();
        hexMesh.triangles = tris_List.ToArray();
        hexMesh.uv = uvs_Arr;

        //Refresh Normals Info
        hexMesh.RecalculateNormals();
        hexMesh.Optimize();

        //Set Mesh To Gameobject
        hexObject_MeshFilter.mesh = hexMesh;
        hexObject_MeshCollider.sharedMesh = hexMesh;
    }

    /////////////////////////////////////////////////////////////////

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

    /////////////////////////////////////////////////////////////////

    public void GenerateHexMesh_Soft_DEBUG()
    {
        Mesh hexMesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        Vector3 center = new Vector3(1f, 0.1f, 0f);
        GenerateAllTriangles_DEBUG(center, vertices, triangles);

        hexMesh.vertices = vertices.ToArray();
        hexMesh.triangles = triangles.ToArray();
        hexMesh.RecalculateNormals();

        hexObject_MeshFilter.mesh = hexMesh;
        hexObject_MeshCollider.sharedMesh = hexMesh;
    }

    private void GenerateAllTriangles_DEBUG(Vector3 center, List<Vector3> vertices, List<int> triangles)
    {

        vertices.Add(center);
        vertices.Add(center + hexMeshCorners[0]);
        vertices.Add(center + hexMeshCorners[1]);
        vertices.Add(center + hexMeshCorners[2]);
        vertices.Add(center + hexMeshCorners[3]);
        vertices.Add(center + hexMeshCorners[4]);
        vertices.Add(center + hexMeshCorners[5]);

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