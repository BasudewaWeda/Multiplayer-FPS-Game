using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketScript : MonoBehaviour
{
    public bool collided;
    public Vector3 hitNormal;

    PhotonView view;

    private void Awake()
    {
        view = GetComponent<PhotonView>();
    }

    private void Update()
    {
        Destroy(gameObject, 10f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        collided = true;
        hitNormal = collision.GetContact(0).normal;
        Destroy(gameObject, 0.0001f);
    }
}
