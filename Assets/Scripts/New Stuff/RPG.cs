using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RPG : Gun
{
    [SerializeField] Camera cam;
    [SerializeField] Transform camHolder;
    [SerializeField] TMP_Text ammoText;

    [SerializeField] GameObject rocketPrefab;
    [SerializeField] GameObject rocketPoint;
    [SerializeField] float rocketSpeed;
    [SerializeField] float blastRadius;
    [SerializeField] GameObject explosionEffect;
    [SerializeField] float damageReductionMultiplier;
    [SerializeField] float damageFallOffDistance;
    [SerializeField] float knockBackForce;

    GameObject rocket;
    Rigidbody rocketRb;
    Transform rocketTr;
    BoxCollider rocketCl;
    RocketScript rocketScript;

    PhotonView view;

    int currentBulletCount;
    bool isReloading;

    Vector3 targetRotation;
    Vector3 currentRotation;
    float recoilX;
    float snappiness;
    float returnSpeed;

    private void Awake()
    {
        view = GetComponent<PhotonView>();
    }

    private void Start()
    {
        currentBulletCount = ((GunInfo)itemInfo).maxBulletCount;
        recoilX = ((GunInfo)itemInfo).recoilX;
        snappiness = ((GunInfo)itemInfo).snappiness;
        returnSpeed = ((GunInfo)itemInfo).returnSpeed;
    }

    private void Update()
    {
        if (!itemGameObject.activeSelf && isReloading)
        {
            StopAllCoroutines();
            isReloading = false;
        }

        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, snappiness * Time.deltaTime);

        if (itemGameObject.activeSelf)
        {
            camHolder.localRotation = Quaternion.Euler(currentRotation);
            ammoText.text = currentBulletCount.ToString() + " / " + ((GunInfo)itemInfo).maxBulletCount.ToString();
        }

        if (rocket != null && view.IsMine)
        {
            if (rocketScript.collided)
            {
                view.RPC(nameof(InstantiateExplosionEffect), RpcTarget.All, rocketTr.position);
                Collider[] colliders = Physics.OverlapSphere(rocketTr.position, blastRadius);

                foreach (Collider coll in colliders)
                {
                    if (coll.gameObject.GetComponent<iDamageable>() != null) // Damage only done by the rocket spawner
                    {
                        float range = (coll.transform.position - rocketTr.position).magnitude;
                        if (range > damageFallOffDistance)
                        {
                            range *= damageReductionMultiplier;
                        }                            
                        else range = 0f;
                        float finalDamage = ((GunInfo)itemInfo).damage - range;
                        coll.gameObject.GetComponent<iDamageable>().TakeDamage(finalDamage);

                        DamagePopUpItem item = coll.transform.GetComponentInChildren<DamagePopUpItem>();
                        if (item != null)
                        {
                            item.UpdateText(finalDamage);
                        }
                        else
                        {
                            DamagePopUpManager.Instance.AddDamageItem(finalDamage, coll.gameObject.transform.position, new Vector3(1f, 2f, 0f), coll);
                        }
                        hitMarkerAnimator.SetTrigger("Hit");
                        ac.PlayOneShot(hitmarkerSound);

                        coll.gameObject.GetComponent<PlayerController>().ExplosionKnockBack(knockBackForce, rocketTr.position, blastRadius);
                    }
                }
                Destroy(rocket);
            }
        }
    }

    public override void Use()
    {
        if (currentBulletCount > 0) Shoot();
    }

    public override void SecondUse()
    {
        Reload();
    }

    void Shoot()
    {
        Recoil();

        gunAnimator.SetTrigger("Shoot");
        view.RPC(nameof(SummonRocket), RpcTarget.All);
        currentBulletCount--;
    }

    void Reload()
    {
        if ((currentBulletCount != ((GunInfo)itemInfo).maxBulletCount) && !isReloading)
        {
            StartCoroutine(ReloadTimer(((GunInfo)itemInfo).reloadTime));
            gunAnimator.SetTrigger("Reload");
        }
    }

    public override IEnumerator ReloadTimer(float delay)
    {
        isReloading = true;
        yield return new WaitForSeconds(delay);
        isReloading = false;
        currentBulletCount = ((GunInfo)itemInfo).maxBulletCount;
    }

    public override void Recoil()
    {
        targetRotation += new Vector3(recoilX, 0f, 0f);
    }

    [PunRPC]
    void SummonRocket()
    {
        rocket = Instantiate(rocketPrefab, rocketPoint.transform.position, rocketPoint.transform.rotation);
        rocketRb = rocket.GetComponent<Rigidbody>();
        rocketTr = rocket.GetComponent<Transform>();
        rocketCl = rocket.GetComponent<BoxCollider>();
        rocketScript = rocket.GetComponent<RocketScript>();
        rocketRb.AddForce(rocketSpeed * rocketTr.forward, ForceMode.Impulse);

        view.RPC(nameof(RPC_ShootEffects), RpcTarget.All);
    }

    [PunRPC]
    void RPC_Shoot(Vector3 hitPosition, Vector3 hitNormal)
    {
        GameObject bulletImpactObj = Instantiate(bulletImpactPrefab, hitPosition + hitNormal * 0.001f, Quaternion.LookRotation(hitNormal, Vector3.up) * bulletImpactPrefab.transform.rotation);
        Destroy(bulletImpactObj, 10f);
    }

    [PunRPC]
    void RPC_ShootEffects()
    {
        ac.PlayOneShot(gunSound);
        muzzleFlash.Play();
    }

    [PunRPC]
    void InstantiateExplosionEffect(Vector3 spawnPos)
    {
        GameObject explosionEffectGameObject =  Instantiate(explosionEffect, spawnPos, Quaternion.identity);
        Destroy(explosionEffectGameObject, 3f);
    } 
}
