using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerpAround : MonoBehaviour
{
    public GameObject targetLerpAround;
    public float speed = 1f;

    // Update is called once per frame
    void Update()
    {
    

        transform.LookAt(targetLerpAround.transform.position);
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }
}
