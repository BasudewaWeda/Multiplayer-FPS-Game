using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AWP : Sniper
{
    [SerializeField] Camera cam;
    [SerializeField] Transform camHolder;
    [SerializeField] TMP_Text ammoText;

    PhotonView view;

    float shootCooldown;
    float shootTimer;
    int currentBulletCount;
    bool isReloading;
    bool canShoot = true;

    Vector3 targetRotation;
    Vector3 currentRotation;
    float recoilX;
    float recoilY;
    float recoilZ;
    float snappiness;
    float returnSpeed;

    private void Awake()
    {
        view = GetComponent<PhotonView>();
    }

    private void Start()
    {
        currentBulletCount = ((SniperInfo)itemInfo).maxBulletCount;
        recoilX = ((SniperInfo)itemInfo).recoilX;
        recoilY = ((SniperInfo)itemInfo).recoilY;
        recoilZ = ((SniperInfo)itemInfo).recoilZ;
        snappiness = ((SniperInfo)itemInfo).snappiness;
        returnSpeed = ((SniperInfo)itemInfo).returnSpeed;
        shootCooldown = ((SniperInfo)itemInfo).shootCooldown;
    }

    private void Update()
    {
        if (!itemGameObject.activeSelf && isReloading)
        {
            StopAllCoroutines();
            Debug.Log("Stop Reloading");
            isReloading = false;
        }

        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, snappiness * Time.deltaTime);

        if (itemGameObject.activeSelf)
        {
            camHolder.localRotation = Quaternion.Euler(currentRotation);
            ammoText.text = currentBulletCount.ToString() + " / " + ((SniperInfo)itemInfo).maxBulletCount.ToString();
            if (!canShoot)
            {
                shootTimer += Time.deltaTime;
            }
        }

        if (shootTimer >= shootCooldown) canShoot = true;
    }

    public override void Use()
    {
        if (CanShoot())
        {
            Shoot();
        }
    }

    public override void SecondUse()
    {
        if (canShoot && !isReloading) Reload();
    }

    void Shoot()
    {
        Recoil();

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        ray.origin = cam.transform.position;
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            gunAnimator.SetTrigger("Shoot");
            view.RPC(nameof(RPC_ShootEffects), RpcTarget.All, hit.point);

            if (hit.collider.gameObject.GetComponent<iDamageable>() != null)
            {
                hit.collider.gameObject.GetComponent<iDamageable>().TakeDamage(((SniperInfo)itemInfo).damage);
                DamagePopUpItem item = hit.transform.GetComponentInChildren<DamagePopUpItem>();
                if (item != null)
                {
                    item.UpdateText(((SniperInfo)itemInfo).damage);
                }
                else
                {
                    DamagePopUpManager.Instance.AddDamageItem(((SniperInfo)itemInfo).damage, hit.collider.gameObject.transform.position, new Vector3(1f, 2f, 0f), hit.collider);
                }
                view.RPC(nameof(RPC_SpawnBloodEffect), RpcTarget.All, hit.point);
                hitMarkerAnimator.SetTrigger("Hit");
                ac.PlayOneShot(hitmarkerSound);
            }
            else
            {
                view.RPC(nameof(RPC_SpawnWallSparks), RpcTarget.All, hit.point, hit.normal);
            }
            view.RPC(nameof(RPC_Shoot), RpcTarget.All, hit.point, hit.normal);
        }

        shootTimer = 0f;
        canShoot = false;
        currentBulletCount--;
        Debug.Log(currentBulletCount);
    }

    bool CanShoot()
    {
        return (currentBulletCount > 0) && !isReloading && canShoot;
    }

    void Reload()
    {
        if ((currentBulletCount != ((SniperInfo)itemInfo).maxBulletCount) && !isReloading)
        {
            StartCoroutine(ReloadTimer(((SniperInfo)itemInfo).reloadTime));
            gunAnimator.SetTrigger("Reload");
            ac.PlayOneShot(reloadSound);
        }
    }

    public override IEnumerator ReloadTimer(float delay)
    {
        isReloading = true;
        Debug.Log("Reloading...");
        yield return new WaitForSeconds(delay);
        isReloading = false;
        currentBulletCount = ((SniperInfo)itemInfo).maxBulletCount;
        Debug.Log("Finished reloading");
    }

    public override void Recoil()
    {
        targetRotation += new Vector3(recoilX, Random.Range(-recoilY, recoilY), Random.Range(-recoilZ, recoilZ));
    }

    [PunRPC]
    void RPC_Shoot(Vector3 hitPosition, Vector3 hitNormal)
    {
        Collider[] colliders = Physics.OverlapSphere(hitPosition, 0.3f);
        if (colliders.Length != 0)
        {
            GameObject bulletImpactObj = Instantiate(bulletImpactPrefab, hitPosition + hitNormal * 0.001f, Quaternion.LookRotation(hitNormal, Vector3.up) * bulletImpactPrefab.transform.rotation);
            Destroy(bulletImpactObj, 10f);
            bulletImpactObj.transform.SetParent(colliders[0].transform);
        }
    }

    [PunRPC]
    void RPC_ShootEffects(Vector3 hitPoint)
    {
        ac.PlayOneShot(gunSound);
        muzzleFlash.Play();
        TrailRenderer trail = Instantiate(bulletTrail, muzzleFlash.transform.position, Quaternion.identity);

        StartCoroutine(SpawnBulletTrail(trail, hitPoint));
    }

    private IEnumerator SpawnBulletTrail(TrailRenderer _trail, Vector3 hitPoint)
    {
        float time = 0f;
        Vector3 startPos = _trail.transform.position;

        while (time < 1)
        {
            _trail.transform.position = Vector3.Lerp(startPos, hitPoint, time);
            time += Time.deltaTime / _trail.time;

            yield return null;
        }

        Destroy(_trail.gameObject, _trail.time);
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
