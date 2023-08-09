using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Melee : Item
{
    public abstract override void Use();
    public abstract override void SecondUse();

    public GameObject knifeImpactPrefab;

    public Animator meleeAnimator;
    public Animator hitMarkerAnimator;

    public AudioSource ac;
    public AudioClip stabSound;
    public AudioClip hitmarkerSound;

    public GameObject bloodSplatEffect;
    public GameObject wallSparksEffect;
}
