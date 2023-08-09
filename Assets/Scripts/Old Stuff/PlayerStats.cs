using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerStats : MonoBehaviour
{
    public float health;
    public float maxHealth;
    [SerializeField] float damageConstant;
    public bool nearDeath;

    [SerializeField] AudioSource ac;
    [SerializeField] AudioClip shotSound;

    PhotonView view; 
    void Start()
    {
        health = maxHealth;

        view = GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!view.IsMine) return;

        if (health - damageConstant <= 0)
        {
            nearDeath = true;
        }

        if (health <= 0)
        {
            Death();
        }
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        ac.PlayOneShot(shotSound);
    }

    private void Death()
    {
        Destroy(gameObject);
    }
}
