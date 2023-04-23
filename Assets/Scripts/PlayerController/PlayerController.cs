using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Rigidbody sphere;
    public Transform boardNormal;
    public Transform boardModel;
    public Transform boardFront;
    public Transform boardTail;

    [Header("Parameters")]
    public float jumpStrength = 10f;
    public float acceleration = 30f;
    public float gravity = 10f;
    public Vector3 boardOffSet;

    [Header("Grounded")]

    [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
    public bool Grounded = true;

    [Tooltip("Useful for rough ground")]
    public float GroundedOffset = 0.1f;

    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    public float GroundedRadius = 0.8f;
    public LayerMask GroundLayers;
    public float slopeLimit;
    public float slopeFriction;
    float slopeAngle;


    [Header("Inputs")]
    public float hInput;
    public float vInput;
    public bool isJumping = false;
    public bool isBreaking = false;


    [Header("State")]
    // States
    public PlayerState CurrentState;

    public PlayerState movementState;
    public PlayerState jumpState;
    public PlayerState breakState;


    public void SetState(PlayerState state)
    {
        if (CurrentState != null)
        {
            CurrentState.OnExit();
        }

        CurrentState = state;
        CurrentState.OnStart();
    }

    void Start()
    {
        movementState = new MovementState(this);
        jumpState = new JumpState(this);
        breakState = new BreakState(this);

        SetState(movementState);
    }

    // Update is called once per frame
    void Update()
    {
        boardModel.position = sphere.position - boardOffSet;

        GetInputs();
        GroundedCheck();

        if (CurrentState != null)
        {
            CurrentState.OnUpdate();
        }

        // Clamp Velocity
        sphere.velocity = Vector3.ClampMagnitude(sphere.velocity, 35f);
    }

    void GetInputs()
    {
        hInput = Input.GetAxis("Horizontal");
        vInput = Mathf.Max(Input.GetAxis("Vertical"), 0);
        isJumping = Input.GetKeyDown(KeyCode.Space);
        isBreaking = Input.GetKey(KeyCode.S);
        Debug.Log("isBreaking: " + isBreaking);
    }


    void GroundedCheck()
    {
        // set sphere position, with offset
        Vector3 spherePosition = new Vector3(boardModel.position.x, boardModel.position.y - GroundedOffset,
            boardModel.position.z + 0.5f);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
            QueryTriggerInteraction.Ignore);

    }



    public void FixBoardYRotationOnGround()
    {
        float rotationSpeed = 60f;
        RaycastHit hit1;
        RaycastHit hit2;
        Quaternion targetRotation;
        if (Physics.Raycast(boardFront.position, Vector3.down, out hit1))
        {
            Vector3 groundNormal = hit1.normal;
            Quaternion groundRotation = Quaternion.FromToRotation(boardModel.up, groundNormal);
            targetRotation = Quaternion.RotateTowards(boardModel.rotation, groundRotation * boardModel.rotation, rotationSpeed * Time.deltaTime);
            boardModel.rotation = targetRotation;
        }
        if (Physics.Raycast(boardTail.position, Vector3.down, out hit2))
        {
            Vector3 groundNormal = hit2.normal;
            Quaternion groundRotation = Quaternion.FromToRotation(boardModel.up, groundNormal);
            targetRotation = Quaternion.RotateTowards(boardModel.rotation, groundRotation * boardModel.rotation, rotationSpeed * Time.deltaTime);
            boardModel.rotation = targetRotation;
        }
    }

    public float OnSlope()
    {
        RaycastHit hit;

        if (Physics.Raycast(boardNormal.position, Vector3.down, out hit))
        {
            slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
            Vector3 slopeDirection = Vector3.Cross(hit.normal, Vector3.down);

            Debug.Log(slopeAngle + " slope direction: " + slopeDirection);
            return slopeAngle;
        }
        else
        {
            return 0;
        }
    }
    public float GetHeight()
    {
        RaycastHit hit;
        Physics.Raycast(boardNormal.position, Physics.gravity, out hit);
        //Debug.DrawRay(boardNormal.position, Physics.gravity, Color.red);
        return hit.distance;
    }

    public void FixRotation()
    {
        RaycastHit hit;
        Physics.Raycast(transform.position, -Vector3.up, out hit);
        var localRot = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
        var euler = localRot.eulerAngles;
        euler.y = 0;
        localRot.eulerAngles = euler;
        boardModel.localRotation = Quaternion.LerpUnclamped(boardModel.localRotation, localRot, 2 * Time.deltaTime);
    }

}