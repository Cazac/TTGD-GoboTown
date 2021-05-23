using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Random = UnityEngine.Random;

public class HexChunk : MonoBehaviour
{
    ////////////////////////////////

    [Header("Hex Chunk Models")]
    public List<MeshFilter> chunkedHexModels_List;

    [Header("Hex Chunk Cells")]
    public HexCell[] hexCellsInChunk_Arr;

    [Header("Hex Chunk Materials")]
    public List<Material> hexChunkMaterials_List;
    List<int> currentMatIDs_List;

    /////////////////////////////////////////////////////////////////

    public void CollectChunkData()
    {
        //Collect All The Hex Cells from the Transform Children of the Object
        hexCellsInChunk_Arr = gameObject.GetComponentsInChildren<HexCell>();
    }

    /////////////////////////////////////////////////////////////////

    public void Chunk(GameObject hexChunkModel_Prefab)
    {
        //Collect Info on which scripts should be included in the chunk
        CollectChunkData();

        //Setup Lists
        List<List<CombineInstance>> combiningListOfLists_List = new List<List<CombineInstance>>();
        chunkedHexModels_List = new List<MeshFilter>();
        currentMatIDs_List = new List<int>();
        //hexChunkMaterials_List = new List<Material>();

        //Loop All Hex Cells TO Merge Meshes
        foreach (HexCell hexCell in hexCellsInChunk_Arr)
        {
            //Check If The Material Has Already Been Added
            if (!currentMatIDs_List.Contains(hexCell.hexCellMatID))
            {
                //Add To The List Of IDed Mats Then Setup a New List For It
                currentMatIDs_List.Add(hexCell.hexCellMatID);
                combiningListOfLists_List.Add(new List<CombineInstance>());
            }

            //Create A Combining Mesh Instance and Fill It
            CombineInstance newCombiningInstance = new CombineInstance();
            newCombiningInstance.mesh = hexCell.hexObject_MeshFilter.sharedMesh;
            newCombiningInstance.transform = hexCell.hexObject_MeshFilter.transform.localToWorldMatrix;

            //Get The position using the Mat ID to locate which list to use and add The Combining Mesh Instance
            int posID = currentMatIDs_List.FindIndex(x => x == hexCell.hexCellMatID);
            combiningListOfLists_List[posID].Add(newCombiningInstance);
        
            //Set The Old Cell Model To Off
            hexCell.hexObject_MeshFilter.gameObject.SetActive(false);
        }

        //Create All Of the Mesh Filter Gameobjects
        for (int i = 0; i < combiningListOfLists_List.Count; i++)
        {
            //Spawn a new Model for each Mat and Record it
            chunkedHexModels_List.Add(Instantiate(hexChunkModel_Prefab, new Vector3(0, 0, 0), Quaternion.identity, gameObject.transform).GetComponent<MeshFilter>());
        }

        for (int i = 0; i < chunkedHexModels_List.Count; i++)
        {
            //Spawn a new Model for each Mat
            chunkedHexModels_List[i].mesh = new Mesh();
            chunkedHexModels_List[i].mesh.CombineMeshes(combiningListOfLists_List[i].ToArray(), true, true);
            chunkedHexModels_List[i].transform.GetComponent<MeshRenderer>().material = SearchMaterialByID(i);
        }

        //Setup The General Info
        chunkedHexModels_List[0].gameObject.name = "Chunk Model";
        chunkedHexModels_List[0].gameObject.transform.SetAsFirstSibling();
    }



    public void Unchunk()
    {
        foreach (MeshFilter chunkedModel in chunkedHexModels_List)
        {
            chunkedModel.gameObject.SetActive(false);
        }

        foreach (HexCell hexCell in hexCellsInChunk_Arr)
        {
            hexCell.hexObject_MeshFilter.gameObject.SetActive(true);
        }
    }

    public void Rechunk()
    {
        foreach (MeshFilter chunkedModel in chunkedHexModels_List)
        {
            chunkedModel.gameObject.SetActive(true);
        } 

        foreach (HexCell hexCell in hexCellsInChunk_Arr)
        {
            hexCell.hexObject_MeshFilter.gameObject.SetActive(false);
        }
    }

    /////////////////////////////////////////////////////////////////

    private Mesh CombineMeshes(MeshFilter[] incomingMeshes)
    {
        // Key: shared mesh instance ID, Value: arguments to combine meshes
        Dictionary<int, List<CombineInstance>> helperDictionary = new Dictionary<int, List<CombineInstance>>();

        // Build combine instances for each type of mesh
        foreach (MeshFilter meshFilter in incomingMeshes)
        {
            List<CombineInstance> combineInstanceLocal_List = new List<CombineInstance>();


            helperDictionary.Add(meshFilter.sharedMesh.GetInstanceID(), combineInstanceLocal_List);


            //Create A New Combine Instance
            CombineInstance meshCombineInstante = new CombineInstance();
            meshCombineInstante.mesh = meshFilter.sharedMesh;
            meshCombineInstante.transform = meshFilter.transform.localToWorldMatrix;

            //Add it to the List
            combineInstanceLocal_List.Add(meshCombineInstante);
        }

        //Debug.Log("Test Code: Count " + helperDictionary.Count);

        //Combine meshes and build combine instance for combined meshes
        List<CombineInstance> combineInstance_List = new List<CombineInstance>();
        foreach (var e in helperDictionary)
        {
            Mesh mesh = new Mesh();
            mesh.CombineMeshes(e.Value.ToArray());
            CombineInstance ci = new CombineInstance();
            ci.mesh = mesh;
            combineInstance_List.Add(ci);
        }

        // And now combine everything
        Mesh result = new Mesh();
        result.CombineMeshes(combineInstance_List.ToArray(), false, false);

        // It is a good idea to clean unused meshes now
        foreach (CombineInstance combineInstance in combineInstance_List)
        {
            Destroy(combineInstance.mesh);
        }

        return result;
    }

    /////////////////////////////////////////////////////////////////

    private Material SearchMaterialByID(int i)
    {
        return hexChunkMaterials_List[currentMatIDs_List[i] - 1];
    }

    public void ColorTheChunk()
    {
        //Not Needed ?????


        /*
        //Get Info
        Mesh mesh = chunkedHexModel1_GO.transform.GetComponent<MeshFilter>().mesh;
        Vector3[] vertices_Arr = mesh.vertices;
        Color[] colors_Arr = new Color[vertices_Arr.Length];

        //Loop through all color Verts
        for (int i = 0; i < colors_Arr.Length; i++)
        {
            //54 is the count of verts on a hex
            colors_Arr[i] = hexCellsInChunk_List[(int)Mathf.Floor(i / 54)].colorActive;
        }

        //Set Color Array on Mesh
        mesh.colors = colors_Arr;

        //Not Needed ??
        mesh.RecalculateNormals();
        */

    }

    public void SetupChunk(int i, int j)
    {
        //chunkedHexModel1_GO = HexModel1_GO;
        //chunkedHexModel2_GO = HexModel2_GO;
        gameObject.name = "Chunk: " + i + "/" + j;
    }

    /////////////////////////////////////////////////////////////////
}
