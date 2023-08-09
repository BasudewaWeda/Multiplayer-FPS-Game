using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ClassSettings : MonoBehaviour
{
    [SerializeField] TMP_Dropdown classDropdown;

    private void Awake()
    {
        classDropdown.value = PlayerPrefs.GetInt("class");
    }

    public void SetClass(int classIndex)
    {
        PlayerPrefs.SetInt("class", classIndex);
    }
}
