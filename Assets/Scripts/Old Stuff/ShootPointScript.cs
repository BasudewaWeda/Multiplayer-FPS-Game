using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ShootPointScript : MonoBehaviour
{
    [SerializeField] LayerMask mask;
    [SerializeField] Transform SOParent;
    [SerializeField] Transform SOChild;

    RaycastHit hit;

    public PhotonView view;

    // Position for Shoot Point
    void Update()
    {
        if (!view.IsMine) return;

        if (Physics.Raycast(SOChild.position, SOParent.position - SOChild.position, out hit, mask))
        {
            transform.position = hit.point;
        }
    }
}
