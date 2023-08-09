using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamagePopUpItem : MonoBehaviour
{
    public TMP_Text damageText;
    public float damage;
    public float fixedSize;
    Camera cam;

    private void Update()
    {
        if (cam == null)
        {
            cam = FindObjectOfType<Camera>();
        }

        if (cam == null)
        {
            return;
        }

        transform.LookAt(cam.transform);
        transform.Rotate(Vector3.up * 180);

        var distance = (cam.transform.position - transform.position).magnitude;
        var size = distance * fixedSize * cam.fieldOfView / 2;
        transform.localScale = Vector3.one * size;
    }

    public void Initialize(float _damage)
    {
        damage += _damage;
        damageText.SetText(_damage.ToString("0"));
        StartCoroutine(DestroyText(1f));
    }

    public void UpdateText(float _damage)
    {
        StopAllCoroutines();
        damage += _damage;
        damageText.SetText(damage.ToString("0"));
        StartCoroutine(DestroyText(1f));
    }

    IEnumerator DestroyText(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}
