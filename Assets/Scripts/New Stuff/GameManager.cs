using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks
{
    [SerializeField] GameObject timerText;
    [SerializeField] GameObject winnerText;
    float startTime;
    [SerializeField] float matchTime;

    [SerializeField] GameObject endGameCamera;

    string winnerName;

    PhotonView view;

    private void Awake()
    {
        view = GetComponent<PhotonView>();
    }

    private void Start()
    {
        PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("starttime", out object t);

        if (t != null)
        {
            startTime = (float)t;
        }
        else
        {
            startTime = matchTime;
            SetTime();
        }
    }

    private void Update()
    {
        if (startTime > 0f) 
        {
            startTime -= Time.deltaTime;
        }
        
        string minutes = ((int)startTime / 60).ToString();
        string seconds = (startTime % 60).ToString("0");

        timerText.GetComponent<TMP_Text>().text = minutes + ":" + seconds;

        if (startTime <= 0f && PhotonNetwork.IsMasterClient)
        {
            view.RPC(nameof(RPC_EndGame), RpcTarget.All);
        }

        if (PhotonNetwork.IsMasterClient)
        {
            SetTime();
        }
    }

    void EndGame()
    {
        int highestKill = 0;

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (PhotonNetwork.PlayerList[i].CustomProperties.TryGetValue("kills", out object kills))
            {
                if (highestKill < (int)kills)
                {
                    highestKill = (int)kills;
                    winnerName = PhotonNetwork.PlayerList[i].NickName;
                }
            }        
        }

        PlayerManager[] playerManagers = FindObjectsOfType<PlayerManager>();
        foreach (PlayerManager playerManager in playerManagers)
        {
            Destroy(playerManager.controller);
        }

        timerText.SetActive(false);
        endGameCamera.SetActive(true);
        winnerText.SetActive(true);

        winnerText.GetComponent<TMP_Text>().text = winnerName + " WON!";

        StartCoroutine(LeaveRoom(2f));
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            SetTime();
        }
    }

    void SetTime()
    {
        Hashtable hash = new Hashtable();
        hash.Add("starttime", startTime);
        PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
    }

    IEnumerator LeaveRoom(float delay)
    {
        yield return new WaitForSeconds(delay);
        ResetStats();
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene(0);
        Cursor.lockState = CursorLockMode.None;
    }

    void ResetStats()
    {
        Hashtable hash = new Hashtable();
        hash.Add("kills", null);
        hash.Add("deaths", null);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }

    [PunRPC]
    void RPC_EndGame()
    {
        EndGame();
    }
}
