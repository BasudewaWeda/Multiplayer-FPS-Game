using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName ="FPS/New Gun")]
public class GunInfo : ItemInfo
{
    [Header("Shooting")]
    public float damage;
    public float fireRate;
    public bool singleFire;

    [Header("Reload")]
    public int maxBulletCount;
    public float reloadTime;

    [Header("Recoil")]
    public float recoilX;
    public float snappiness;
    public float returnSpeed;

    [Header("Spread")]
    public bool spread;
    public float spreadAmount;
    public int pelletPerShot;
    public float damageFallOffRange;
}
