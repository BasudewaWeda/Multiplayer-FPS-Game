using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class Knife : Melee
{
    [SerializeField] Camera cam;

    PhotonView view;
    bool canUse = true;

    private void Awake()
    {
        view = GetComponent<PhotonView>();
    }

    public override void Use()
    {
        if (canUse)
        {
            Stab();
        }
    }

    public override void SecondUse()
    {
        Use();
    }

    void Stab()
    {
        meleeAnimator.SetTrigger("Stab");
        view.RPC(nameof(RPC_StabSound), RpcTarget.All);
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        ray.origin = cam.transform.position;
        if (Physics.Raycast(ray, out RaycastHit hit, ((MeleeInfo)itemInfo).range))
        {
            if (hit.collider.gameObject.GetComponent<iDamageable>() != null)
            {
                hit.collider.gameObject.GetComponent<iDamageable>().TakeDamage(((MeleeInfo)itemInfo).damage);
                DamagePopUpItem item = hit.transform.GetComponentInChildren<DamagePopUpItem>();
                if (item != null)
                {
                    item.UpdateText(((MeleeInfo)itemInfo).damage);
                }
                else
                {
                    DamagePopUpManager.Instance.AddDamageItem(((MeleeInfo)itemInfo).damage, hit.collider.gameObject.transform.position, new Vector3(1f, 2f, 0f), hit.collider);
                }
                view.RPC(nameof(RPC_SpawnBloodEffect), RpcTarget.All, hit.point);
                hitMarkerAnimator.SetTrigger("Hit");
                ac.PlayOneShot(hitmarkerSound);
            }
            else
            {
                view.RPC(nameof(RPC_SpawnWallSparks), RpcTarget.All, hit.point, hit.normal);
            }
            view.RPC(nameof(RPC_Slash), RpcTarget.All, hit.point, hit.normal);
        }

        StartCoroutine(StabCooldown(((MeleeInfo)itemInfo).cooldown));
    }

    IEnumerator StabCooldown(float delay)
    {
        canUse = false;
        yield return new WaitForSeconds(delay);
        canUse = true;
    }

    [PunRPC]
    void RPC_Slash(Vector3 hitPosition, Vector3 hitNormal)
    {
        Collider[] colliders = Physics.OverlapSphere(hitPosition, 0.3f);
        if (colliders.Length != 0)
        {
            GameObject bulletImpactObj = Instantiate(knifeImpactPrefab, hitPosition + hitNormal * 0.001f, Quaternion.LookRotation(hitNormal, Vector3.up) * knifeImpactPrefab.transform.rotation);
            Destroy(bulletImpactObj, 10f);
            bulletImpactObj.transform.SetParent(colliders[0].transform);
        }
    }

    [PunRPC]
    void RPC_StabSound()
    {
        ac.PlayOneShot(stabSound);
    }

    [PunRPC]
    void RPC_SpawnBloodEffect(Vector3 spawnPos)
    {
        GameObject bloodEffect = Instantiate(bloodSplatEffect, spawnPos, Quaternion.identity);
        Destroy(bloodEffect, 1f);
    }

    [PunRPC]
    void RPC_SpawnWallSparks(Vector3 hitPos, Vector3 hitNormal)
    {
        GameObject wallSparks = Instantiate(wallSparksEffect, hitPos, Quaternion.LookRotation(hitNormal, Vector3.up));
        Destroy(wallSparks, 1f);
    }
}
