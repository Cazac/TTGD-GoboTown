using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Random = UnityEngine.Random;

public class HexChunk : MonoBehaviour
{
    ////////////////////////////////

    public enum ChunkState
    {
        Active,
        Inactive,

        Loading,
        Chunking,

        Unchunking,
        Unloading,
    }

    ////////////////////////////////

    [Header("Hex Chunk Current State")]
    public ChunkState currentState = ChunkState.Inactive;
    public HexChunkCoords chunkCoords;

    [Header("Hex Cells / Combined Meshes In Chunk")]
    public HexCell[] hexCellsInChunk_Arr;
    public List<MeshFilter> hexCellMergedMeshes_List;

    [Header("Hex Chunk Materials")]
    private List<Material> hexChunkMaterials_List;
    private List<Tuple<int, int>> currentMatIDs_List;

    /////////////////////////////////////////////////////////////////

    public void SetChunkActive(HexChunkCoords newChunkCoords, HexCell_Data[,] chunkDataCells_Arr)
    {
        //Setup Chunk Name & State
        chunkCoords = newChunkCoords;
        gameObject.name = "Chunk: " + chunkCoords.x + "/" + chunkCoords.y;
        currentState = ChunkState.Active;

        //Get ChunkSize Once
        int chunkSize = MapSpawnController.Instance.mapGenOpts_SO.mapGen_ChunkSize;

        //Loop and Purpose All Cells
        for (int y = 0; y < chunkSize; y++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                //Get HexCell that is next in the array
                HexCell newHexCell = hexCellsInChunk_Arr[(y * chunkSize) + x];

                //Use the Data On That Cell
                newHexCell.RePurposeCell(chunkDataCells_Arr[x, y]);
            }
        }

        //Chunk();
    }

    /////////////////////////////////////////////////////////////////

    public void Chunk()
    {
        //Setup Lists
        List<List<CombineInstance>> combiningListOfLists_List = new List<List<CombineInstance>>();
        hexCellMergedMeshes_List = new List<MeshFilter>();
        currentMatIDs_List = new List<Tuple<int, int>>();
        hexChunkMaterials_List = new List<Material>();

        //Loop All Hex Cells To Merge Meshes
        foreach (HexCell hexCell in hexCellsInChunk_Arr)
        {
            //Check If The Material Has Already Been Added
            if (!currentMatIDs_List.Contains(new Tuple<int, int>(hexCell.hexCell_BiomeID, hexCell.hexCell_MatID)))
            {
                //Add To The List Of IDed Mats Then Setup a New List For It
                currentMatIDs_List.Add(new Tuple<int, int>(hexCell.hexCell_BiomeID, hexCell.hexCell_MatID));
                combiningListOfLists_List.Add(new List<CombineInstance>());
            }

            //Create A Combining Mesh Instance and Fill It
            CombineInstance newCombiningInstance = new CombineInstance();
            newCombiningInstance.mesh = hexCell.hexObject_MeshFilter.sharedMesh;
            newCombiningInstance.transform = hexCell.hexObject_MeshFilter.transform.localToWorldMatrix;

            //Get The position using the Mat ID to locate which list to use and add The Combining Mesh Instance
            int posID = currentMatIDs_List.FindIndex(x => x.Item1 == hexCell.hexCell_BiomeID && x.Item2 == hexCell.hexCell_MatID);
            combiningListOfLists_List[posID].Add(newCombiningInstance);
        
            //Set The Old Cell Model To Off
            hexCell.hexObject_MeshFilter.gameObject.SetActive(false);
        }



        //Create All Of the Mesh Filter Gameobjects
        for (int i = 0; i < combiningListOfLists_List.Count; i++)
        {
            //Spawn a new Merged Chunk Model for each Mat and Record it
            hexCellMergedMeshes_List.Add(Instantiate(MapSpawnController.Instance.hexChunkModel_Prefab, new Vector3(0, 0, 0), Quaternion.identity, gameObject.transform).GetComponent<MeshFilter>());
        }



        for (int i = 0; i < hexCellMergedMeshes_List.Count; i++)
        {
            //Spawn a new Model for each Mat
            hexCellMergedMeshes_List[i].mesh = new Mesh();
            hexCellMergedMeshes_List[i].mesh.CombineMeshes(combiningListOfLists_List[i].ToArray(), true, true);
            hexCellMergedMeshes_List[i].transform.GetComponent<MeshRenderer>().material = MapSpawnController.Instance.GetBiomeMaterial(currentMatIDs_List[i].Item1, currentMatIDs_List[i].Item2);
        }



        //Setup The General Info
        hexCellMergedMeshes_List[0].gameObject.name = "Chunk Merged Model";
        hexCellMergedMeshes_List[0].gameObject.transform.SetAsFirstSibling();
    }

    public void Unchunk()
    {
        foreach (MeshFilter chunkedModel in hexCellMergedMeshes_List)
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
        foreach (MeshFilter chunkedModel in hexCellMergedMeshes_List)
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

    public void Poolable_OnSpawn()
    {
        //Set Parent
        gameObject.transform.SetParent(HexPoolingController.Instance.hexChunk_ActiveContainer.transform);
    }

    public void Poolable_OnDespawn()
    {
        //Set Parent
        gameObject.transform.SetParent(HexPoolingController.Instance.hexChunk_InactiveContainer.transform);
    }

    /////////////////////////////////////////////////////////////////
}
