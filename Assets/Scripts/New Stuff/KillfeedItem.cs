using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System;

public class KillfeedItem : MonoBehaviourPunCallbacks
{
    [Serializable]
    public class KeyValuePair
    {
        public string itemName;
        public Sprite itemImage;
    }

    public List<KeyValuePair> list = new List<KeyValuePair>();
    Dictionary<string, Sprite> spritesDict = new Dictionary<string, Sprite>();

    public TMP_Text killerText;
    public TMP_Text killedText;
    [SerializeField] Image weaponImage;

    private void Awake()
    {
        foreach (var kvp in list)
        {
            spritesDict[kvp.itemName] = kvp.itemImage;
        }
    }

    private void Update()
    {
        Destroy(gameObject, 3f);
    }

    public void Initialize(Player killer, Player killed, string itemName)
    {
        killerText.text = killer.NickName;
        killedText.text = killed.NickName;
        weaponImage.sprite = spritesDict[itemName];
    }
}
