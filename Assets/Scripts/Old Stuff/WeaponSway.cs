using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class WeaponSway : MonoBehaviour
{
    [SerializeField] float smooth;
    [SerializeField] float swayMultiplier;
    float mouseX;
    float mouseY;
    Quaternion xRotation;
    Quaternion yRotation;

    float moveHorizontal;
    float moveVertical;
    float jumpMove;

    Rigidbody rb;

    PhotonView view;

    PlayerController pc;

    [SerializeField] Item item;

    [SerializeField] AudioSource ac;
    [SerializeField] AudioClip reloadSound;
    [SerializeField] AudioClip awpBoltSound;

    private void Awake()
    {
        view = transform.root.GetComponent<PhotonView>();
        rb = transform.root.GetComponent<Rigidbody>();
        pc = transform.root.GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!view.IsMine || !item.itemGameObject.activeSelf) return;

        mouseX = Input.GetAxisRaw("Mouse X") * swayMultiplier * (pc.mouseSensitivity / 200);
        mouseY = Input.GetAxisRaw("Mouse Y") * swayMultiplier * (pc.mouseSensitivity / 200);

        xRotation = Quaternion.AngleAxis(mouseY, Vector3.right);
        yRotation = Quaternion.AngleAxis(-mouseX, Vector3.up);

        Quaternion targetRotation = xRotation * yRotation;

        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, smooth * Time.deltaTime);

        moveHorizontal = Input.GetAxisRaw("Horizontal") * swayMultiplier/25;
        moveVertical = Input.GetAxisRaw("Vertical") * swayMultiplier/25;
        jumpMove = rb.velocity.y / 50;

        Vector3 targetPosition = new(moveHorizontal, jumpMove, moveVertical);

        transform.localPosition = Vector3.Lerp(transform.localPosition, -targetPosition, smooth * Time.deltaTime);
    }

    public void PlayReloadSound()
    {
        ac.PlayOneShot(reloadSound);
    }

    public void PlayAWPBoltSound()
    {
        ac.PlayOneShot(awpBoltSound);
    }
}
