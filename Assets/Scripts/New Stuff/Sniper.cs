using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Sniper : Item
{
    public abstract override void Use();

    public abstract override void SecondUse();

    public abstract IEnumerator ReloadTimer(float delay);

    public abstract void Recoil();

    public GameObject bulletImpactPrefab;

    public Animator gunAnimator;
    public Animator hitMarkerAnimator;

    public AudioSource ac;
    public AudioClip gunSound;
    public AudioClip reloadSound;
    public AudioClip hitmarkerSound;

    public ParticleSystem muzzleFlash;
    public GameObject bloodSplatEffect;
    public TrailRenderer bulletTrail;
    public GameObject wallSparksEffect;
}
