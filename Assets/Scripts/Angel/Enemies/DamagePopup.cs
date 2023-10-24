using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    private TextMeshPro _textMesh;
    private readonly float _speed = 4f;
    private float _lifetimeTimer; //time before popup starts disappearing
    private const float LifetimeTimerMax = 1f;
    private readonly float _changeScaleAmount = 1;
    private Color _color;
    private Vector3 _moveVector;

    public static DamagePopup Create(GameObject textDamagePopup, Vector3 position, int damageAmount)
    {
        Transform popupTransform = Instantiate(textDamagePopup.transform, position, Quaternion.identity);
        DamagePopup damagePopup = popupTransform.GetComponent<DamagePopup>();
        damagePopup.Setup(damageAmount);
        return damagePopup;
    }

    // Start is called before the first frame update
    void Awake()
    {
        _textMesh = transform.GetComponent<TextMeshPro>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += _moveVector * Time.deltaTime;
        _moveVector -= _moveVector * (5f * Time.deltaTime);

        if (_lifetimeTimer > (LifetimeTimerMax / 2)) //if we are on the first half of the popup
        {
            transform.localScale += Vector3.one * (_changeScaleAmount * Time.deltaTime);
        }
        else //second half
        {
            transform.localScale -= Vector3.one * (_changeScaleAmount * Time.deltaTime);
        }

        _lifetimeTimer -= Time.deltaTime;
        if (_lifetimeTimer <= 0)
        {
            float disappearSpeed = 3f;
            _color.a -= disappearSpeed * Time.deltaTime;
            _textMesh.color = _color;
            if (_color.a <= 0)
            {
                Destroy(gameObject);
            }
        }
    }

    void Setup(int damageAmount)
    {
        _textMesh.SetText(damageAmount.ToString());
        _color = _textMesh.color;
        _lifetimeTimer = LifetimeTimerMax;
        _moveVector = new Vector3(1, 1) * _speed;
    }
}
