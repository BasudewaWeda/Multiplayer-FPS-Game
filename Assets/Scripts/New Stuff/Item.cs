using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item : MonoBehaviour
{
    public ItemInfo itemInfo;
    public GameObject itemGameObject;

    public abstract void Use(); // Shoot for guns, stab for melees
    public abstract void SecondUse(); // Reload for guns, idk for melees
}
