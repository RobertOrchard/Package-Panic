using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    [SerializeField] List<Transform> faces = new();

    [SerializeField] float lookSensitivity;
    [SerializeField] float moveForce = 10f;
    [SerializeField] float baseJumpForce = 3f;
    [Range(0f, 1f), SerializeField] float airControlMult = 0.1f;
    [SerializeField] float jumpCooldown = .4f;
    float elapsedSinceJump = 0f;
    [SerializeField] float maxCoyoteTime = 0.1f;
    float elapsedCTime = 0f;

    Rigidbody rb;
    CameraController camCont = null;
    private InputSystem_Actions input;

    private Global global = null;

    Transform jumpFace = null;
    bool onGround = true;
    int digits = 0; // how many digits in the volume when measured from grams (kg / 100)

    private void Awake()
    {
        input = new();
        input.Enable();

        rb = GetComponent<Rigidbody>();
        camCont = Camera.main.GetComponent<CameraController>();

        RecalculateEdge();

        StartCoroutine(DelayedStart());
    }

    IEnumerator DelayedStart()
    {
        yield return null;
        global = Global.Instance;
    }

    private void FixedUpdate()
    {
        if (global != null && global.IsPaused) return;

        Movement();
    }

    #region Controls
    Vector2 lastMoveInput = Vector2.zero;
    private void Movement()
    {
        Vector2 moveInput = input.Player.Move.ReadValue<Vector2>();
        lastMoveInput = moveInput;

        if (moveInput == Vector2.zero) return;

        if (!onGround) moveInput *= airControlMult;

        //Vector3 baseForce = moveForce * Time.fixedDeltaTime * -Vector3.up;

        if (moveInput.x != 0f) // left/right
        {
            /*Vector3 _force = -Mathf.Abs(moveInput.x) * baseForce * rb.mass;
            Vector3 _position = transform.position +
                furthestEdgeDist * (Global.Instance.GlobalRight * (moveInput.x < 0f ? 1f : -1f));
            rb.AddForceAtPosition(_force, _position, ForceMode.Force); // creates a torque*/

            Vector3 force = moveInput.x * moveForce * rb.mass * Time.fixedDeltaTime * global.GlobalRight;
            rb.AddForce(force, ForceMode.Force);
        }
        if (moveInput.y != 0f) // forward/backward
        {
            /*Vector3 _force = -Mathf.Abs(moveInput.y) * baseForce * rb.mass;
            Vector3 _position = transform.position +
                furthestEdgeDist * (Global.Instance.GlobalForward * (moveInput.y < 0f ? 1f : -1f));
            rb.AddForceAtPosition(_force, _position, ForceMode.Force);*/

            Vector3 force = moveInput.y * moveForce * rb.mass * Time.fixedDeltaTime * global.GlobalForward;
            rb.AddForce(force, ForceMode.Force);
        }
    }

    private void Jump()
    {
        if(elapsedSinceJump < jumpCooldown)
        {
            elapsedSinceJump += Time.deltaTime;
            return;
        }

        if(jumpFace == null) return;
        if (!onGround) return;
        if(!input.Player.Jump.WasPressedThisFrame()) return;

        Vector3 upDir = (jumpFace.position - transform.position).normalized;
        rb.AddForce(rb.mass * (digits + baseJumpForce) * upDir, ForceMode.Impulse);

        elapsedCTime = maxCoyoteTime;
        elapsedSinceJump = 0f;
    }

    private void Look()
    {
        if (!camCont) return;

        Vector2 lookInput = input.Player.Look.ReadValue<Vector2>();

        if(lookInput.x != 0f)
        {
            camCont.RotateYaw(lookInput.x * lookSensitivity * Time.deltaTime);
        }
        if(lookInput.y != 0f)
        {
            camCont.RotatePitch(-lookInput.y * lookSensitivity * Time.deltaTime);
        }
    }
    #endregion

    private void Update()
    {
        if (global == null || global.IsPaused) return;

        CalculateJumpFace();
        Jump();
        Look();

        GroundCheck();
    }

    // Finds best face to use in jump calculation
    void CalculateJumpFace()
    {
        Vector3 targetPos = transform.position;
        targetPos += lastMoveInput.x * .5f * global.GlobalRight;
        targetPos += lastMoveInput.y * .5f * global.GlobalForward;
        //targetPos += new Vector3(lastMoveInput.x, 0f, lastMoveInput.y) * .5f; // doesn't use cameraspace
        targetPos.y += .3f;

        int bestIndex = -1;
        float lowestDist = Mathf.Infinity;

        for(int i = 0; i < faces.Count; i++)
        {
            float distance = Vector3.Magnitude(targetPos - faces[i].position);
            if(distance < lowestDist)
            {
                lowestDist = distance;
                bestIndex = i;
            }
        }

        if (bestIndex < 0) return;
        //Debug.Log("Best Face Score " + lowestDist, faces[bestIndex].gameObject);

        jumpFace = faces[bestIndex];
    }

    Vector3 debugPos = Vector3.zero;
    float furthestEdgeDist;
    void GroundCheck()
    {
        Vector3 endPos = transform.position;
        endPos += Vector3.down * furthestEdgeDist;
        bool isOnGrnd = Physics.Raycast(transform.position, Vector3.down, furthestEdgeDist);

        if (isOnGrnd)
        {
            elapsedCTime = 0f;
            onGround = true;
        }
        else if(elapsedCTime < maxCoyoteTime)
        {
            elapsedCTime += Time.deltaTime;
        }
        else
        {
            onGround = false;
        }

        debugPos = endPos;
    }

    public void DeniedPickup(Vector3 objectPosition)
    {
        Vector3 launchNormal = Vector3.ProjectOnPlane(transform.position - objectPosition, Vector3.up).normalized;

        rb.AddForce(rb.mass * (digits + baseJumpForce) * launchNormal, ForceMode.Impulse);
    }

    public void RecalculateEdge()
    {
        Vector3 offset1 = transform.position - faces[0].position;
        Vector3 offset2 = transform.position - faces[1].position;
        Vector3 offset3 = transform.position - faces[2].position;
        furthestEdgeDist = (offset1 + offset2 + offset3).magnitude * 1.1f; // dist from center to a corner
    }

    /*// for debug drawing
    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, debugPos);
    }*/

    public void CalculateDigits(float valInMetersCubed)
    {
        digits = Mathf.FloorToInt(Mathf.Log10(valInMetersCubed * 100f) + 1);
        Debug.Log(digits);
    }
}
