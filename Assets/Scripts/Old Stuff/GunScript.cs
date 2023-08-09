using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GunScript : MonoBehaviour
{
    [SerializeField] Transform orientation;
    [SerializeField] Transform bulletPoint;
    [SerializeField] GameObject bullet;

    [SerializeField] bool canShoot;
    [SerializeField] float timeBetweenShots;
    [SerializeField] LayerMask head;
    [SerializeField] LayerMask body;
    
    float damage;
    [SerializeField] float headDamage;
    [SerializeField] float bodyDamage;

    public int bulletCount;
    public int maxBullet;
    [SerializeField] float reloadTime;
    [SerializeField] bool canReload;
    [SerializeField] bool currentlyReloading;

    [SerializeField] ParticleSystem shootingParticle;
    [SerializeField] Animator anim;
    [SerializeField] Animator hitMarkerAnim;
    [SerializeField] Animator skullAnim;

    [SerializeField] AudioSource ac;
    [SerializeField] AudioSource ac2;
    [SerializeField] AudioClip gunSound;
    [SerializeField] AudioClip bodyShotSound;
    [SerializeField] AudioClip headShotSound;
    [SerializeField] AudioClip reloadSound;
    [SerializeField] AudioClip killSound;

    RaycastHit hit;

    public PhotonView view;

    // Start is called before the first frame update
    void Start()
    {
        bulletCount = maxBullet;
        canShoot = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!view.IsMine) return;

        if (Input.GetMouseButtonDown(0) && canShoot && !currentlyReloading)
        {
            Shoot();
        }

        if (bulletCount < maxBullet) canReload = true;
        else canReload = false;

        if (canReload && Input.GetKeyDown(KeyCode.R) && !currentlyReloading)
        {
            Reload();
        }
    }

    void Shoot()
    {
        canShoot = false;
        bulletCount--;
        Instantiate(bullet, bulletPoint.position, bulletPoint.rotation);
        StartCoroutine(ShootDelay(timeBetweenShots));
        shootingParticle.Play();
        anim.SetTrigger("Shoot");
        ac.PlayOneShot(gunSound);

        if (Physics.Raycast(orientation.position, orientation.forward, out hit, body))
        {
            if (hit.collider.GetType() == typeof(SphereCollider))
            {
                damage = headDamage;
                ac2.PlayOneShot(headShotSound);
                hitMarkerAnim.SetTrigger("Head");
                Kill();
            }
            else if (hit.collider.GetType() == typeof(CapsuleCollider))
            {
                damage = bodyDamage;
                ac2.PlayOneShot(bodyShotSound);
                hitMarkerAnim.SetTrigger("Body");
            }

            PlayerStats ps = hit.transform.GetComponent<PlayerStats>();

            if (ps != null)
            {
                ps.TakeDamage(damage);
                if (ps.nearDeath) Kill();
            }
        }

        Debug.DrawRay(orientation.position, orientation.forward * 2f, Color.red, 5f);
    }

    void Reload()
    {
        anim.SetTrigger("Reload");
        ac.PlayOneShot(reloadSound);
        StartCoroutine(Reload(reloadTime));
    }

    IEnumerator Reload(float delay)
    {
        currentlyReloading = true;
        canReload = false;
        yield return new WaitForSeconds(delay);
        currentlyReloading = false;
        bulletCount = maxBullet;
        StartCoroutine(ShootDelay(timeBetweenShots));
    }

    IEnumerator ShootDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (bulletCount > 0) canShoot = true;
    }

    void Kill()
    {
        skullAnim.SetTrigger("Kill");
        bulletCount = maxBullet;
        ac2.PlayOneShot(killSound);
    }
}
