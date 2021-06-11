using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkPoolingController : MonoBehaviour
{
    /////////////////////////////////////////////////////////////////

    public static ChunkPoolingController Instance;

    /////////////////////////////////////////////////////////////////

    [Header("Sets the startup amount of meshes more will spawn if needed")]
    public int intialPoolCount;

    [Header("Chunk Prefab")]
    public GameObject chunk_Prefab;

    [Header("Containers")]
    public GameObject activeChunks_Container;
    public GameObject inactiveChunks_Container;

    [Header("Queues")]
    public Queue<HexChunk> activepool_Queue;
    public Queue<HexChunk> inactivepool_Queue;

    /////////////////////////////////////////////////////////////////

    private void Awake()
    {
        Instance = this;

        activepool_Queue = new Queue<HexChunk>();
        inactivepool_Queue = new Queue<HexChunk>();
    }

    private void Start()
    {
        for (int i = 0; i < intialPoolCount; i++)
        {
            inactivepool_Queue.Enqueue(Instantiate(chunk_Prefab, inactiveChunks_Container.transform).GetComponent<HexChunk>());
        }
    }

    /////////////////////////////////////////////////////////////////

    public void SetChunk(Mesh newMesh)
    {
        //Spawn New Gameobject if Empty Queue
        if (inactivepool_Queue.Count == 0)
        {
            inactivepool_Queue.Enqueue(Instantiate(chunk_Prefab, inactiveChunks_Container.transform).GetComponent<HexChunk>());
        }

        HexChunk poolableObject = inactivepool_Queue.Dequeue();
        activepool_Queue.Enqueue(poolableObject);
        poolableObject.Poolable_OnSpawn();
    }

    public void ClearAllMeshes()
    {
        int counter = activepool_Queue.Count;
        for (int i = 0; i < counter; i++)
        {
            HexChunk poolableObject = activepool_Queue.Dequeue();
            inactivepool_Queue.Enqueue(poolableObject);
            poolableObject.Poolable_OnDespawn();
        }
    }

    /////////////////////////////////////////////////////////////////
}
