using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamagePopUpManager : MonoBehaviour
{
    public static DamagePopUpManager Instance;

    [SerializeField] GameObject damagePopupPrefab;

    private void Awake()
    {
        Instance = this;
    }

    public void AddDamageItem(float damage, Vector3 spawnPos, Vector3 offset, Collider parent)
    {
        DamagePopUpItem item = Instantiate(damagePopupPrefab, spawnPos + offset, Quaternion.identity).GetComponent<DamagePopUpItem>();
        item.Initialize(damage);
        item.transform.SetParent(parent.transform);
    }
}
