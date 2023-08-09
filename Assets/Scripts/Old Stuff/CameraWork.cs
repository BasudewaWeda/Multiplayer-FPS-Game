using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CameraWork : MonoBehaviour
{
    public PhotonView view;

    public Camera mainCam;
    // Start is called before the first frame update
    void Start()
    {
        mainCam = GetComponent<Camera>();
        if (!view.IsMine) mainCam.enabled = false;
    }
}
