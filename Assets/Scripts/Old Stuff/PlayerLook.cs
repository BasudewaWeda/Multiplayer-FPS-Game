using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerLook : MonoBehaviour
{
    [SerializeField] Transform orientation;
    [SerializeField] Transform shootOrientation;
    public PlayerMovement player;
    private float mouseX;
    private float mouseY;
    private float xRotation;
    private float yRotation;
    [SerializeField] private float mouseSensitivity;

    public Camera mainCamera;
    [SerializeField] float tilt;
    [SerializeField] float zoomOut;
    [SerializeField] float zoomIn;
    [SerializeField] float defaultZoom;
    [SerializeField] float smooth;
    float tiltAmount;
    float zoomAmount;

    [SerializeField] Transform camTransform;
    [SerializeField] Rigidbody rb;
    [SerializeField] float lowestYSpeed;
    [SerializeField] float currentLowestYSpeed;
    [SerializeField] float bounceAmount;

    public PhotonView view;

    // Start is called before the first frame update
    void Start()
    {
        zoomAmount = defaultZoom;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        if (!view.IsMine) return;

        mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity * Time.deltaTime;
        mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        yRotation += mouseX;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, yRotation, tiltAmount);
        orientation.rotation = Quaternion.Euler(0f, yRotation, 0f);
        shootOrientation.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
        mainCamera.fieldOfView = zoomAmount;

        if (player.isWallRunning || player.isSliding)
        {
            zoomAmount = Mathf.Lerp(zoomAmount, zoomOut, smooth * Time.deltaTime);
            if (player.rightWall)
            {
                tiltAmount = Mathf.Lerp(tiltAmount, tilt, smooth * Time.deltaTime);
            }
            else if (player.leftWall)
            {
                tiltAmount = Mathf.Lerp(tiltAmount, -tilt, smooth * Time.deltaTime);
            }
        }
        else if (player.isWallClimbing)
        {
            zoomAmount = Mathf.Lerp(zoomAmount, zoomIn, smooth * Time.deltaTime);
        }

        zoomAmount = Mathf.Lerp(zoomAmount, defaultZoom, smooth * Time.deltaTime);
        tiltAmount = Mathf.Lerp(tiltAmount, 0, smooth * Time.deltaTime);

        if (rb.velocity.y <= 0)
        {
            currentLowestYSpeed = rb.velocity.y;
            if (currentLowestYSpeed < lowestYSpeed)
            {
                lowestYSpeed = currentLowestYSpeed;
            }
        }

        if (player.isGrounded) StartCoroutine(CameraFeedBack(0.25f));
    }

    IEnumerator CameraFeedBack(float delay)
    {
        bounceAmount = Mathf.Lerp(bounceAmount, lowestYSpeed/25, smooth * Time.deltaTime);
        camTransform.localPosition = new Vector3(0f, bounceAmount, 0f);
        yield return new WaitForSeconds(delay);
        bounceAmount = Mathf.Lerp(bounceAmount, 0, smooth * Time.deltaTime);
        camTransform.localPosition = new Vector3(0f, bounceAmount, 0f);
        lowestYSpeed = 0f;
    }
}
