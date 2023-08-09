using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UsernameDisplay : MonoBehaviour
{
    [SerializeField] PhotonView view;
    [SerializeField] TMP_Text text;

    private void Start()
    {
        if (view.IsMine)
        {
            gameObject.SetActive(false);
        }
        text.text = view.Owner.NickName;
    }
}
