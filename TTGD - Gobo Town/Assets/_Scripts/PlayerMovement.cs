using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///////////////
/// <summary>
///     
/// 
/// </summary>
///////////////

public class PlayerMovement : MonoBehaviour
{
    ////////////////////////////////

    public static PlayerMovement Instance;

    ////////////////////////////////



    [Header("Cameras")]
    public Camera cameraFirstPerson;
    public Camera cameraThirdPerson;

    [Header("Players")]
    public GameObject playerFirstPerson;
    public GameObject playerThirdPerson;

    [Header("Player's Charecter Controller")]
    public CharacterController playerCharController;

    [Header("Camera Speeds")]
    //public float speedFirstPerson = 1f;
    public float speedThirdPerson = 1f;

    [Header("Mouse Controls")]
    public float mouseSensitivity = 1f;

    ////////////////////////////////

    [Header("Player Speeds")]
    public float defaultSpeed = 10f;
    public float sprintSpeed = 15f;

    ////////////////////////////////

    [Header("Mouse Controls")]
    private Vector2 defaultLookLimits = new Vector2(-90f, 90f);
    private Vector3 startingRotation;
    private Vector2 lookAngles;
    private float currentRollAngle;

    ////////////////////////////////

    [Header("Current Player Force")]
    private float currentSpeed;
    private float verticalVelocity;
    private Vector3 moveDirection;

    [Header("Constant Forces")]
    private readonly float jumpForce = 12f;
    private readonly float gravityForce = 30f;

    ///////////////////////////////////////////////////////

    private void Awake()
    {
        //Set Static Singleton Self Refference
        Instance = this;

        //Record Starting Angle
        startingRotation = gameObject.transform.rotation.eulerAngles;
    }

    private void Start()
    {
        //Check Camera Type
        if (MapSpawnController.Instance.mapGenOpts_SO.isCameraFirstPerson)
        {
            //Lock Mouse to Game
            LockMouse();
            cameraFirstPerson.gameObject.SetActive(true);
            cameraThirdPerson.gameObject.SetActive(false);

            playerFirstPerson.SetActive(true);
            playerThirdPerson.SetActive(false);
        }
        else if (MapSpawnController.Instance.mapGenOpts_SO.isCameraThirdPerson)
        {
            //Lock Mouse to Game
            UnlockMouse();
            cameraFirstPerson.gameObject.SetActive(false);
            cameraThirdPerson.gameObject.SetActive(true);

            playerFirstPerson.SetActive(false);
            playerThirdPerson.SetActive(true);
        }
    }

    private void Update()
    {
        //Check Locking State
        UpdateCheck_CameraLockState();

        if (MapSpawnController.Instance.mapGenOpts_SO.isCameraFirstPerson)
        {
            //First Person
            MoveCamera_FirstPerson();
        }
        else if (MapSpawnController.Instance.mapGenOpts_SO.isCameraThirdPerson)
        {
            //Third Person
            MoveCamera_ThirdPerson();
        }
    }

    ///////////////////////////////////////////////////////

    private void UpdateCheck_CameraLockState()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.None)
            {
                LockMouse();
            }
            else
            {
                UnlockMouse();
            }
        }
    }

    private void LockMouse()
    {
        //Hide Cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UnlockMouse()
    {
        //Allow Cusror to show
        //Not Needed anymore?
        //Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    ///////////////////////////////////////////////////////

    private void MoveCamera_FirstPerson()
    {
        if (Cursor.lockState == CursorLockMode.None)
        {
            return;
        }

        //Get Mouse Speeds
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        //Combine Mouse inputs
        Vector2 currentMouseLook = new Vector2(-mouseY, mouseX);

        //Current Angle Modded by the sensitivity
        lookAngles.x += currentMouseLook.x * mouseSensitivity;
        lookAngles.y += currentMouseLook.y * mouseSensitivity;

        //clamp look angle to min and max look angles
        lookAngles.x = Mathf.Clamp(lookAngles.x, defaultLookLimits.x, defaultLookLimits.y);

        //Set new angles
        cameraFirstPerson.gameObject.transform.localRotation = Quaternion.Euler(lookAngles.x, 0f, currentRollAngle);
        playerFirstPerson.gameObject.transform.localRotation = Quaternion.Euler(0f, lookAngles.y + startingRotation.y, 0f);

        //Get Player Movement Values
        PlayerSprint();
        PlayerMove_Regluar();
        PlayerMove_Vertical();

        //Move the player
        playerCharController.Move(moveDirection);
    }

    private void MoveCamera_ThirdPerson()
    {
        //Create Input Direction
        Vector3 inputMOvement = new Vector3(0, 0, 0);

        //Gathering Input Info
        if (Input.GetKey(KeyCode.W))
        {
            inputMOvement += new Vector3(0, 0, 1f);
        }
        if (Input.GetKey(KeyCode.S))
        {
            inputMOvement += new Vector3(0, 0, -1f);
        }
        if (Input.GetKey(KeyCode.A))
        {
            inputMOvement += new Vector3(-1f, 0, 0);
        }
        if (Input.GetKey(KeyCode.D))
        {
            inputMOvement += new Vector3(1f, 0, -0);
        }

        //Shift Speed Up
        if (Input.GetKey(KeyCode.LeftShift))
        {
            cameraThirdPerson.transform.position += inputMOvement.normalized * speedThirdPerson * Time.deltaTime * 2f;
        }
        else
        {
            cameraThirdPerson.transform.position += inputMOvement.normalized * speedThirdPerson * Time.deltaTime;
        }
    }

    ///////////////////////////////////////////////////////

    private float GetGravity()
    {
        //Return a downwards position
        return verticalVelocity -= gravityForce * Time.deltaTime;
    }

    private float PlayerJump()
    {
        if (playerCharController.isGrounded && Input.GetKey(KeyCode.Space))
        {
            return jumpForce;
        }
        else
        {
            return verticalVelocity;
        }
    }

    private void PlayerSprint()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed = sprintSpeed;
        }
        else
        {
            currentSpeed = defaultSpeed;
        }
    }

    private void PlayerMove_Vertical()
    {
        //Set new Y velcoity from gravity + jump
        verticalVelocity = GetGravity();
        verticalVelocity = PlayerJump();

        //Set Y value to move direction
        moveDirection.y = verticalVelocity * Time.deltaTime;
    }

    private void PlayerMove_Regluar()
    {
        // use Unity's built-in Input axis to create a move vector
        moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));

        // change moveDirection from local space to global space (current global pos + moveDirection)
        moveDirection = cameraFirstPerson.transform.TransformDirection(moveDirection);
        moveDirection *= currentSpeed * Time.deltaTime;
    }

    ///////////////////////////////////////////////////////
}