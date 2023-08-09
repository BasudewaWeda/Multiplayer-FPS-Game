using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;
using Photon.Realtime;
using System.Linq;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using TMPro;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    PhotonView PV;

    public GameObject controller;

    [Header("Respawns")]
    AudioSource ac;
    [SerializeField] AudioClip spawnSound;
    [SerializeField] float respawnTime;
    [SerializeField] TMP_Text respawnTimerText;
    float respawnTimer;
    bool dead;
    bool canSpawn;
    [SerializeField] Toggle autoRespawnToggle;
    bool autoRespawn;
    [SerializeField] GameObject deathEffect;

    [Header("Class")]
    [SerializeField] string[] classes;
    [SerializeField] TMP_Dropdown classDropdown;

    [Header("DeathCam")]
    [SerializeField] GameObject deathCam;
    [SerializeField] GameObject UI;
    [SerializeField] TMP_Text killedByText;

    Transform killerTransform;

    int kills;
    int deaths;
    string itemName;

    [Header("MultiKills")]
    [SerializeField] float multiKillTime;
    float multiKillTimer;
    int currentMultiKill;
    int currentKillStreak;

    void Awake()
    {
        PV = GetComponent<PhotonView>();
        ac = GetComponent<AudioSource>();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (PV.IsMine)
        {
            CreateController();
            classDropdown.value = PlayerPrefs.GetInt("class");
            if (PlayerPrefs.GetInt("autoRespawn") == 1)
            {
                autoRespawnToggle.isOn = true;
                autoRespawn = true;
            }
            else
            {
                autoRespawnToggle.isOn = false;
                autoRespawn = false;
            }
        }
        else if (!PV.IsMine)
        {
            Destroy(ac);
            Destroy(deathCam);
            Destroy(UI);
        }
    }

    void Update()
    {
        if (!PV.IsMine) return;

        if (controller != null)
        {
            transform.position = controller.transform.position;
            if (currentMultiKill > 0)
            {
                multiKillTimer += Time.deltaTime;
            }

            if (multiKillTimer >= multiKillTime)
            {
                multiKillTimer = 0f;
                currentMultiKill = 0;
            }
        }

        if (deathCam.activeSelf && deathCam != null)
        {
            deathCam.transform.LookAt(killerTransform.position);
        }

        if (dead)
        {
            respawnTimer -= Time.deltaTime;
            respawnTimerText.text = "Respawn in " + respawnTimer.ToString("0");
        }

        if (respawnTimer <= 0f && dead)
        {
            canSpawn = true;
            respawnTimerText.text = "Press Space To Respawn";
        }

        if (canSpawn && (Input.GetKeyDown(KeyCode.Space) || autoRespawn))
        {
            CreateController();
            canSpawn = false;
            dead = false;
        }
    }

    void CreateController()
    {
        DeathCam(false);
        Transform spawnPoint = SpawnManager.Instance.GetSpawnPoint();
        if (PlayerPrefs.HasKey("class"))
        {
            controller = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", classes[PlayerPrefs.GetInt("class")]), spawnPoint.position, spawnPoint.rotation, 0, new object[] { PV.ViewID });
        }
        else
        {
            controller = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", classes[0]), spawnPoint.position, spawnPoint.rotation, 0, new object[] { PV.ViewID });
        }
        PV.RPC(nameof(RPC_PlaySpawnSound), RpcTarget.All);
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void Die(Player player)
    {
        DeathCam(true);
        dead = true;
        canSpawn = false;
        respawnTimer = respawnTime;
        killerTransform = Find(player).gameObject.transform;
        killedByText.text = "Killed by " + player.NickName;
        PhotonNetwork.Destroy(controller);
        PV.RPC(nameof(RPC_DeathEffect), RpcTarget.All);

        deaths++;
        currentKillStreak = 0;
        currentMultiKill = 0;
        multiKillTimer = 0f;

        Hashtable hash = new Hashtable();
        hash.Add("deaths", deaths);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);

        Cursor.lockState = CursorLockMode.None;
    }

    public void GetKill(Player player, bool suicide)
    {
        if (!suicide) PV.RPC(nameof(RPC_GetKill), PV.Owner);

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (PhotonNetwork.PlayerList[i].ActorNumber == PV.OwnerActorNr)
            {
                itemName = (string)PhotonNetwork.PlayerList[i].CustomProperties["itemName"];
            }
        }

        PV.RPC(nameof(RPC_UpdateKillFeed), RpcTarget.All, player, itemName);
    }

    [PunRPC]
    void RPC_GetKill(PhotonMessageInfo info)
    {
        kills++;
        currentKillStreak++;
        currentMultiKill++;
        multiKillTimer = 0f;

        ac.Play();

        controller.GetComponent<PlayerController>().currentHealth = controller.GetComponent<PlayerController>().maxHealth;

        Hashtable hash = new Hashtable();
        hash.Add("kills", kills);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);

        if (currentKillStreak > 2)
        {
            if (currentKillStreak > 10) currentKillStreak = 10;
            PV.RPC(nameof(RPC_UpdateKillStreak), RpcTarget.All, currentKillStreak - 3);
        }

        if (currentMultiKill > 1)
        {
            if (currentMultiKill > 5) currentMultiKill = 5;
            if (currentKillStreak > 2)
            {
                PV.RPC(nameof(RPC_UpdateMultiKill), RpcTarget.All, currentMultiKill - 2, 2.5f);
                return;
            }
            PV.RPC(nameof(RPC_UpdateMultiKill), RpcTarget.All, currentMultiKill - 2, 0f);
        }
    }

    [PunRPC]
    void RPC_UpdateKillFeed(Player killed, string itemName)
    {
        KillfeedManager.Instance.AddKillfeedItem(PV.Owner, killed, itemName);
    }

    [PunRPC]
    void RPC_UpdateKillStreak(int currentKillStreak)
    {
        KillStreaksMultiKillsManager.Instance.AddKillStreakItem(PV.Owner, currentKillStreak);
    }

    [PunRPC]
    void RPC_UpdateMultiKill(int currentMultiKill, float delay)
    {
        KillStreaksMultiKillsManager.Instance.AddMulltiKillItem(PV.Owner, currentMultiKill, delay);
    }

    [PunRPC]
    void RPC_PlaySpawnSound()
    {
        ac.PlayOneShot(spawnSound);
    }

    public static PlayerManager Find(Player player)
    {
        return FindObjectsOfType<PlayerManager>().SingleOrDefault(x => x.PV.Owner == player);
    }

    void DeathCam(bool activation = false)
    {
        if (!PV.IsMine) return;
        deathCam.SetActive(activation);
        UI.SetActive(activation);
    }

    public void SetClass(int classIndex)
    {
        PlayerPrefs.SetInt("class", classIndex);
    }

    public void SetAutoRespawn(bool _autoRespawn)
    {
        autoRespawn = _autoRespawn;
        if (_autoRespawn) PlayerPrefs.SetInt("autoRespawn", 1);
        else if (!_autoRespawn) PlayerPrefs.SetInt("autoRespawn", 0); 
    }

    [PunRPC]
    void RPC_DeathEffect()
    {
        GameObject _deathEffect = Instantiate(deathEffect, transform.position, Quaternion.identity);
        Destroy(_deathEffect, 1f);
    }
}
