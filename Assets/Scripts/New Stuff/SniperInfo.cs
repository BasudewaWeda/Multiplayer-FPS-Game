using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FPS/New Sniper")]
public class SniperInfo : ItemInfo
{
    [Header("Shooting")]
    public float damage;
    public float shootCooldown;

    [Header("Reload")]
    public float reloadTime;
    public int maxBulletCount;

    [Header("Recoil")]
    public float recoilX;
    public float recoilY;
    public float recoilZ;
    public float snappiness;
    public float returnSpeed;
}
