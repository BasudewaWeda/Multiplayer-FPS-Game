using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerMovement : MonoBehaviour
{
    PhotonView view;
    public Transform orientation;
    [SerializeField] Rigidbody rb;
    public LayerMask ground;

    private float moveHorizontal;
    private float moveVertical;
    Vector3 moveDirection;
    [SerializeField] float moveSpeed;
    [SerializeField] float speedSmooth;
    [SerializeField] float airMultiplier;
    [SerializeField] float groundDrag;
    float desiredSpeed;
    float currentMoveSpeed;

    public bool isGrounded;

    [SerializeField] float jumpForce;
    [SerializeField] float fallingMultiplier;

    [SerializeField] float wallCheckDistance;
    [SerializeField] float minJumpHeight;
    [SerializeField] float wallRunGravity;
    [SerializeField] float sideWallJumpForce;
    [SerializeField] float wallJumpForce;
    [SerializeField] bool isAboveGround;
    private RaycastHit rightWallhit;
    private RaycastHit leftWallhit;
    public bool rightWall;
    public bool leftWall;
    public bool isWallRunning;

    bool frontWall;
    public bool isWallClimbing;
    RaycastHit frontWallHit;
    [SerializeField] float climbForce;
    [SerializeField] float backJumpForce;

    private RaycastHit slopeHit;
    bool onSlope;
    [SerializeField] float maxSlopeAngle;

    [SerializeField] float maxSlideTimer;
    [SerializeField] float slideSpeed;
    [SerializeField] float slideYScale;
    [SerializeField] float startYScale;
    [SerializeField] float slideTimer;
    public bool isSliding;

    void Start()
    {
        view = GetComponent<PhotonView>();        
    }
    // Update is called once per frame
    void Update()
    {
        if (!view.IsMine) return;

        moveHorizontal = Input.GetAxisRaw("Horizontal");
        moveVertical = Input.GetAxisRaw("Vertical");

        isGrounded = IsGrounded();
        isAboveGround = AboveGround();
        onSlope = OnSlope();

        if (isGrounded)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = 0;
        }

        if (Input.GetKeyDown(KeyCode.LeftControl) && (moveHorizontal != 0 || moveVertical != 0) && isGrounded)
        {
            ShrinkPlayer();
        }

        if (Input.GetKeyUp(KeyCode.LeftControl) && isSliding)
        {
            UnShrinkPlayer();
        }
    }

    void FixedUpdate()
    {
        if (!view.IsMine) return;

        if (moveHorizontal != 0 || moveVertical != 0)
        {
            if (isSliding)
            {
                desiredSpeed = slideSpeed;
            }
            else
            {
                desiredSpeed = moveSpeed;
            }
        }

        currentMoveSpeed = Mathf.Lerp(currentMoveSpeed, desiredSpeed, speedSmooth * Time.deltaTime);

        moveDirection = orientation.forward * moveVertical + orientation.right * moveHorizontal; // Calculating movement

        if (isWallClimbing && isAboveGround && (moveVertical > 0))
        {
            rb.AddForce(climbForce * moveVertical * Vector3.up, ForceMode.Force);
        }
        else if (onSlope && !Input.GetKey(KeyCode.Space))
        {
            rb.AddForce(Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized * currentMoveSpeed, ForceMode.Force);
            if (rb.velocity.y > 0)
            {
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }
        else if (!isWallRunning && isGrounded && !onSlope)
        {
            rb.AddForce(moveDirection.normalized * currentMoveSpeed, ForceMode.Force);
        }
        else if (!isWallRunning && !isGrounded)
        {
            rb.AddForce(moveDirection.normalized * currentMoveSpeed * airMultiplier, ForceMode.Force);
        }

        Vector3 currentSpeed = new(rb.velocity.x, 0f, rb.velocity.z);

        if (currentSpeed.magnitude >= (currentMoveSpeed/10)) // Speed Limit
        {
            Vector3 limitedSpeed = currentSpeed.normalized * (currentMoveSpeed/10);
            rb.velocity = new Vector3(limitedSpeed.x, rb.velocity.y, limitedSpeed.z);
        }

        if (Input.GetKey(KeyCode.Space) && isGrounded)
        {
            Jump();
        }

        if (rb.velocity.y <= 0) // Fall faster
        {
            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y + (rb.velocity.y * fallingMultiplier), rb.velocity.z);
        }

        rightWall = Physics.Raycast(transform.position, orientation.right, out rightWallhit, wallCheckDistance, ground);
        leftWall = Physics.Raycast(transform.position, -orientation.right, out leftWallhit, wallCheckDistance, ground);
        frontWall = Physics.SphereCast(transform.position, .5f, orientation.forward, out frontWallHit, wallCheckDistance, ground);

        if ((rightWall || leftWall) && moveVertical > 0 && isAboveGround) // Wallrun
        {
            isWallRunning = true;

            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

            Vector3 wallNormal = rightWall ? rightWallhit.normal : leftWallhit.normal;

            Vector3 wallForward = Vector3.Cross(wallNormal, Vector3.up);

            if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
            {
                wallForward = -wallForward;
            }

            rb.AddForce(wallForward * currentMoveSpeed + wallRunGravity * Vector3.down, ForceMode.Force);

            if (Input.GetKey(KeyCode.Space))
            {
                WallJump();
            }
        }
        else
        {
            isWallRunning = false;
        }

        if (isSliding)
        {
            if (!onSlope || (onSlope && (rb.velocity.y > 0)))
            {
                Vector3 slideDirection = orientation.forward * moveVertical + orientation.right * moveHorizontal;

                rb.AddForce(slideDirection.normalized * currentMoveSpeed, ForceMode.Force);

                slideTimer += Time.deltaTime;
            }
            else if (onSlope && rb.velocity.y < 0)
            {
                Debug.Log("bruh");
                rb.AddForce(Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized * currentMoveSpeed, ForceMode.Force);
            }

            if (slideTimer >= maxSlideTimer)
            {
                UnShrinkPlayer();
            }
        }

        if (frontWall && !(leftWall || rightWall) && isAboveGround) // Wall climbing
        {
            isWallClimbing = true;
            if (Input.GetKey(KeyCode.Space))
            {
                BackWallJump();
            }
            else if (!Input.GetKey(KeyCode.Space))
            {
                rb.velocity = new Vector3(0, rb.velocity.y, 0);
            }
        }
        else if (!frontWall)
        {
            isWallClimbing = false;
        }
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, -Vector3.up, 1.2f, ground);
    }

    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, -Vector3.up, minJumpHeight, ground);
    }

    private void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void WallJump()
    {
        Vector3 wallNormal = rightWall ? rightWallhit.normal : leftWallhit.normal;

        Vector3 appliedForce = Vector3.up * wallJumpForce + wallNormal * sideWallJumpForce;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(appliedForce, ForceMode.Impulse);
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, 1.2f, ground))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    private void ShrinkPlayer() // Start Slide
    {
        isSliding = true;
        transform.localScale = new Vector3(transform.localScale.x, slideYScale, transform.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
    }

    private void UnShrinkPlayer() // Stop Slide
    {
        isSliding = false;
        transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        slideTimer = 0;
    }

    private void BackWallJump()
    {
        Vector3 wallNormal = frontWallHit.normal;

        Vector3 appliedForce = Vector3.up * wallJumpForce + wallNormal * backJumpForce;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(appliedForce, ForceMode.Impulse);
    }
}
