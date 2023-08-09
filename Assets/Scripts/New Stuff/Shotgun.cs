using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class Shotgun : Gun
{
    [SerializeField] Camera cam;
    [SerializeField] Transform camHolder;
    [SerializeField] TMP_Text ammoText;

    PhotonView view;

    float timeBetweenShots;
    int currentBulletCount;
    bool isReloading;
    bool isSingleFire;

    Vector3 targetRotation;
    Vector3 currentRotation;
    float recoilX;
    float snappiness;
    float returnSpeed;

    [SerializeField] TrailRenderer bulletTrail;
    [SerializeField] GameObject bloodSplatEffect;
    [SerializeField] GameObject wallSparksEffect;

    private void Awake()
    {
        view = GetComponent<PhotonView>();
    }

    private void Start()
    {
        currentBulletCount = ((GunInfo)itemInfo).maxBulletCount;
        isSingleFire = ((GunInfo)itemInfo).singleFire;
        recoilX = ((GunInfo)itemInfo).recoilX;
        snappiness = ((GunInfo)itemInfo).snappiness;
        returnSpeed = ((GunInfo)itemInfo).returnSpeed;
    }

    private void Update()
    {
        timeBetweenShots += Time.deltaTime;

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
    }

    public override void Use()
    {
        if (CanShoot())
        {
            if (isSingleFire && Input.GetMouseButtonDown(0)) Shoot();
            else if (!isSingleFire) Shoot();
        }
    }

    public override void SecondUse()
    {
        Reload();
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
                hit.collider.gameObject.GetComponent<iDamageable>().TakeDamage(((GunInfo)itemInfo).damage);
                DamagePopUpItem item = hit.transform.GetComponentInChildren<DamagePopUpItem>();
                if (item != null)
                {
                    item.UpdateText(((GunInfo)itemInfo).damage);
                }
                else
                {
                    DamagePopUpManager.Instance.AddDamageItem(((GunInfo)itemInfo).damage, hit.collider.gameObject.transform.position, new Vector3(1f, 2f, 0f), hit.collider);
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

        timeBetweenShots = 0f;
        currentBulletCount--;
    }

    bool CanShoot()
    {
        return (currentBulletCount > 0) && !isReloading && (timeBetweenShots >= 1f / (((GunInfo)itemInfo).fireRate / 60f));
    }

    void Reload()
    {
        if ((currentBulletCount != ((GunInfo)itemInfo).maxBulletCount) && !isReloading)
        {
            StartCoroutine(ReloadTimer(((GunInfo)itemInfo).reloadTime));
            gunAnimator.SetTrigger("Reload");
            ac.PlayOneShot(reloadSound);
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
