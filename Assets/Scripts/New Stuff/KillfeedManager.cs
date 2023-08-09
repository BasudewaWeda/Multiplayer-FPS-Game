using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;

public class KillfeedManager : MonoBehaviour
{
    public static KillfeedManager Instance;

    [SerializeField] GameObject killfeedItemPrefab;
    [SerializeField] Transform container;

    private void Awake()
    {
        Instance = this;
    }

    public void AddKillfeedItem(Player killer, Player killed, string itemName)
    {
        KillfeedItem item = Instantiate(killfeedItemPrefab, container).GetComponent<KillfeedItem>();
        item.Initialize(killer, killed, itemName);
    }
}
