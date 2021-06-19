using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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
        new Vector3(0f, 0f, MapSpawnController.outerRadius),
        new Vector3(MapSpawnController.innerRadius, 0f, 0.5f * MapSpawnController.outerRadius),
        new Vector3(MapSpawnController.innerRadius, 0f, -0.5f * MapSpawnController.outerRadius),
        new Vector3(0f, 0f, -MapSpawnController.outerRadius),
        new Vector3(-MapSpawnController.innerRadius, 0f, -0.5f * MapSpawnController.outerRadius),
        new Vector3(-MapSpawnController.innerRadius, 0f, 0.5f * MapSpawnController.outerRadius),
        new Vector3(0f, 0f, MapSpawnController.outerRadius)
    };

    ////////////////////////////////

    [Header("Visable Hex Coords")]
    public HexCellCoords hexCoords;

    [Header("Debug Visual Labels")]
    public TextMeshProUGUI hexLabel_Text;

    [Header("Hex Object Information")]
    public GameObject hexObject_GO;
    public MeshFilter hexObject_MeshFilter;
    public MeshRenderer hexObject_MeshRenderer;
    public MeshCollider hexObject_MeshCollider;
    public GameObject IDCanvas;

    [Header("Hex Color Options")]
    public Color hexCellColor_Main;

    public int hexCell_BiomeID;
    public int hexCell_MatID;
    public int hexCell_heightSteps;
    public float hexCell_TotalHeight;

    /////////////////////////////////////////////////////////////////

    public void ClickCell()
    {
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

    public void SpawnCell()
    {
        //Create Mesh
        GenerateHexMesh_HardStyle();
    }

    public void RePurposeCell(HexCell_Data hexCell_Data)
    {
        //Create Hex Labels
        string hexID = "Hex Cell " + (hexCell_Data.hexCoords.x.ToString() + "/" + hexCell_Data.hexCoords.y.ToString());
        hexLabel_Text.text = hexID;
        gameObject.name = hexID;

        //Update Height Set
        UpdateCoords(hexCell_Data);

        //Update Height Set
        UpdatePosition(hexCell_Data);

        //Update Material
        UpdateMaterial(hexCell_Data.hexCell_BiomeID, hexCell_Data.hexCell_MatID);

        //Create a color from the data and set it
        ColorUtility.TryParseHtmlString("#" + hexCell_Data.hexCell_Color, out Color convertedColor);
        UpdateCellColor(convertedColor);
    }

    /////////////////////////////////////////////////////////////////

    private void UpdateCoords(HexCell_Data hexCell_Data)
    {
        hexCoords = new HexCellCoords(hexCell_Data.hexCoords.x, hexCell_Data.hexCoords.y, hexCell_Data.hexCoords.l);
    }

    private void UpdatePosition(HexCell_Data hexCell_Data)
    {
        //Set Saved Height
        hexCell_heightSteps = hexCell_Data.hexCell_HeightSteps;

        //Calculate Height and Set Current Value
        hexCell_TotalHeight = MapSpawnController.Instance.mapGenOpts_SO.mapGen_HeightPerStep * hexCell_heightSteps;

        //Set the Gameobject Value
        gameObject.transform.position = new Vector3(gameObject.transform.position.x, hexCell_TotalHeight, gameObject.transform.position.z);

        //Regular Spawn Position VS Offset Spacing
        if (hexCoords.x % 2 == 0)
        {
            float posX = hexCoords.x * MapSpawnController.spacing_J;
            float posY = hexCoords.y * MapSpawnController.spacing_I;
            float posH = MapSpawnController.Instance.mapGenOpts_SO.mapGen_HeightPerStep * hexCell_heightSteps;

            //Set World Position Of the Cell
            gameObject.transform.localPosition = new Vector3(posX, posH, posY);
        }
        else
        {
            float posX = hexCoords.x * MapSpawnController.spacing_J;
            float posY = hexCoords.y * MapSpawnController.spacing_I + MapSpawnController.offcenter_I;
            float posH = MapSpawnController.Instance.mapGenOpts_SO.mapGen_HeightPerStep * hexCell_heightSteps;

            //Set World Position Of the Cell
            gameObject.transform.localPosition = new Vector3(posX, posH, posY);
        }


        /*
        //Regular Spawn Position VS Offset Spacing
        if (hexCoords.y % 2 == 0)
        {
            float posY = hexCoords.x * MapSpawnController.spacing_I;
            float posX = hexCoords.y * MapSpawnController.spacing_J;
            float posH = MapSpawnController.Instance.mapGenOpts_SO.mapGen_HeightPerStep * hexCell_heightSteps;

            //Set World Position Of the Cell
            gameObject.transform.localPosition = new Vector3(posX, posH, posY);
        }
        else
        {
            float posY = hexCoords.x * MapSpawnController.spacing_I + MapSpawnController.offcenter_I;
            float posX = hexCoords.y * MapSpawnController.spacing_J;
            float posH = MapSpawnController.Instance.mapGenOpts_SO.mapGen_HeightPerStep * hexCell_heightSteps;

            //Set World Position Of the Cell
            gameObject.transform.localPosition = new Vector3(posX, posH, posY);
        }
         * */
    }

    private void UpdateMaterial(int matID_Biome, int matID_Mat)
    {
        //Set Mat IDs
        hexCell_BiomeID = matID_Biome;
        hexCell_MatID = matID_Mat;

        //Set Renderer Color
        hexObject_MeshRenderer.material = MapSpawnController.Instance.GetBiomeMaterial(hexCell_BiomeID, hexCell_MatID);
    }

    private void UpdateCellColor(Color newColor)
    {
        //Get Mesh and Create Array
        Mesh mesh = hexObject_MeshFilter.mesh;
        Color[] colors_Arr = new Color[mesh.vertices.Length];

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

    private void GenerateHexMesh_HardStyle()
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

    private void GenerateMesh_Top(List<Vector3> vertices, List<int> triangles)
    {
        //Vector3 center = cell.transform.localPosition;
        for (int i = 0; i < 6; i++)
        {
            Vector3 center = new Vector3(0f, 0.1f, 0f);

            GenerateMesh_CreateTriangle(
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
            GenerateMesh_CreateTriangle(
                center + hexMeshCorners[i] + height,
                center + hexMeshCorners[i + 1],
                center + hexMeshCorners[i],
                vertices,
                triangles
            );
        }



        for (int i = 0; i < 6; i++)
        {
            GenerateMesh_CreateTriangle(
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

    private void GenerateMesh_CreateTriangle(Vector3 v1, Vector3 v2, Vector3 v3, List<Vector3> vertices, List<int> triangles)
    {
        int vertexIndex = vertices.Count;
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
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
}