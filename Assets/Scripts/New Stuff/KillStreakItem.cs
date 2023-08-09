using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Realtime;

public class KillStreakItem : MonoBehaviour
{
    [SerializeField] TMP_Text killStreakText;
    [SerializeField] string[] killStreaks;

    private void Update()
    {
        Destroy(gameObject, 3f);
    }

    public void Initialize(Player player ,int killStreakIndex)
    {
        killStreakText.text = player.NickName + " " + killStreaks[killStreakIndex];
    }
}
