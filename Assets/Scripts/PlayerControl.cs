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

    [SerializeField] float heavyImpactThreshold = 3.5f;
    [SerializeField] GameObject heavyImpactParticle;

    Rigidbody rb;
    CameraController camCont = null;
    private InputSystem_Actions input;

    private Global global = null;

    Vector3 lastVelocity = Vector3.zero;
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

    private void OnDestroy()
    {
        input?.Disable();
    }

    private void FixedUpdate()
    {
        if (global != null && global.IsPaused) return;

        Movement();

        lastVelocity = rb.linearVelocity;
    }

    #region Controls
    Vector2 lastMoveInput = Vector2.zero;
    private void Movement()
    {
        Vector2 moveInput = input.Player.Move.ReadValue<Vector2>();
        lastMoveInput = moveInput;

        if (moveInput == Vector2.zero) return;

        if (!onGround) moveInput *= airControlMult;

        if (moveInput.x != 0f) // left/right
        {
            Vector3 force = moveInput.x * moveForce * rb.mass * Time.fixedDeltaTime * global.GlobalRight;
            rb.AddForce(force, ForceMode.Force);
        }
        if (moveInput.y != 0f) // forward/backward
        {
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

        if (!onGround) return;
        if(!input.Player.Jump.WasPressedThisFrame()) return;

        rb.AddForce(rb.mass * (digits + baseJumpForce) * Vector3.up, ForceMode.Impulse);

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

        Jump();
        Look();

        GroundCheck();
    }

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
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(lastVelocity.magnitude > heavyImpactThreshold)
        {
            ContactPoint point = collision.GetContact(0);
            GameObject inst = Instantiate(heavyImpactParticle, point.point, Quaternion.LookRotation(point.normal));
            inst.transform.localScale = transform.lossyScale;
        }
    }

    #region Public Funcs
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

    public void CalculateDigits(float valInMetersCubed)
    {
        digits = Mathf.FloorToInt(Mathf.Log10(valInMetersCubed * 100f) + 1);
    }
    #endregion
}
