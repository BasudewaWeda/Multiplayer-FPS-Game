using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class UIScript : MonoBehaviour
{
    [SerializeField] GunScript gs;
    [SerializeField] PlayerStats ps;

    [SerializeField] TextMeshProUGUI ammoCount;

    [SerializeField] Slider healthBar;

    public PhotonView view;
    // Start is called before the first frame update
    void Start()
    {
        healthBar.maxValue = ps.maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (!view.IsMine) return;

        ammoCount.text = gs.bulletCount.ToString() + " / " + gs.maxBullet.ToString();

        healthBar.value = ps.health;
    }
}
