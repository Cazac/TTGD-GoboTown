using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraExplorer : MonoBehaviour
{



    float speed = 1f;



    // Update is called once per frame
    void Update()
    {

        Input_MoveCamera();

    }


    private void Input_MoveCamera()
    {
        Vector3 inputMOvement = new Vector3(0,0,0);

        if (Input.GetKey(KeyCode.W))
        {
            inputMOvement += new Vector3(0,0,1f);
        }

        if (Input.GetKey(KeyCode.S))
        {
            inputMOvement += new Vector3(0, 0, -1f);
        }




        Camera.main.transform.position += inputMOvement.normalized * speed;


    }
}
