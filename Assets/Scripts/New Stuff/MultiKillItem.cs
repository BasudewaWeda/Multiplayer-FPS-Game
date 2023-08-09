using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using TMPro;

public class MultiKillItem : MonoBehaviour
{
    [SerializeField] TMP_Text multiKillText;
    [SerializeField] string[] multiKills;

    private void Update()
    {
        Destroy(gameObject, 3f);
    }

    public void Initialize(Player player, int multiKillIndex)
    {
        multiKillText.text = player.NickName + " " + multiKills[multiKillIndex];
    }
}
