using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;

public class KillStreaksMultiKillsManager : MonoBehaviour
{
    public static KillStreaksMultiKillsManager Instance;

    [SerializeField] GameObject multiKillPrefab;
    [SerializeField] GameObject killStreakPrefab;
    [SerializeField] Transform container;
    [SerializeField] AudioClip[] killStreakSound;
    [SerializeField] AudioClip[] multiKillSound;
    PhotonView view;
    AudioSource ac;

    private void Awake()
    {
        Instance = this;
        view = GetComponent<PhotonView>();
        ac = GetComponent<AudioSource>();
    }

    public void AddMulltiKillItem(Player player, int currentMultiKill, float delay)
    {
        MultiKillItem item = Instantiate(multiKillPrefab, container).GetComponent<MultiKillItem>();
        item.Initialize(player, currentMultiKill);
        view.RPC(nameof(PlayMultiKillSound), RpcTarget.All, currentMultiKill, delay);
    }

    public void AddKillStreakItem(Player player, int currentKillStreak)
    {
        KillStreakItem item = Instantiate(killStreakPrefab, container).GetComponent<KillStreakItem>();
        item.Initialize(player, currentKillStreak);
        view.RPC(nameof(PlayKillStreakSound), RpcTarget.All, currentKillStreak);
    }

    [PunRPC]
    void PlayKillStreakSound(int currentKillStreak)
    {
        StopAllCoroutines();
        ac.PlayOneShot(killStreakSound[currentKillStreak]);
    }

    [PunRPC]
    void PlayMultiKillSound(int currentMultiKill, float delay)
    {
        StartCoroutine(DelaySound(delay, currentMultiKill));
    }

    IEnumerator DelaySound(float delay, int currentMultiKill)
    {
        yield return new WaitForSeconds(delay);
        ac.PlayOneShot(multiKillSound[currentMultiKill]);
    }
}
