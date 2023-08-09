using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class TurnOff : MonoBehaviour
{
    public PhotonView view;
    public GameObject mainCam;
    public GameObject UI;
    public Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        if (!view.IsMine)
        {
            Destroy(mainCam);
            Destroy(UI);
            Destroy(rb);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
