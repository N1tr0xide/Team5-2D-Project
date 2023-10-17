using System;
using System.Collections;
using System.Collections.Generic;
using Angel;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PlayerUIController : MonoBehaviour
{
    [SerializeField] private Image[] uiHearths;
    [SerializeField] private Image[] uiBullets;
    [SerializeField] private PlayerController playerController;

    [SerializeField] private Sprite fullHearth;
    [SerializeField] private Sprite emptyHearth;
    
    // Start is called before the first frame update
    void Start()
    {
        playerController.OnHealthChanged += PlayerController_OnHealthChanged; //subscribe to events
        playerController.OnAmmoChanged += PlayerController_onAmmoChanged;
    }

    #region events
        private void PlayerController_onAmmoChanged(int ammo)
        {
            UpdateAmmoUI(ammo);
        }

        private void PlayerController_OnHealthChanged(int health)
        {
            UpdateHeartsUI(health);
        }
    #endregion
    
    void UpdateHeartsUI(int currentHealth)
    {
        int health = currentHealth;
        
        foreach (Image image in uiHearths)
        {
            if (health > 0)
            {
                image.sprite = fullHearth;
                health--;
            }
            else { image.sprite = emptyHearth; }
        }
    }

    void UpdateAmmoUI(int currentAmmo)
    {
        int ammo = currentAmmo;

        foreach (Image image in uiBullets)
        {
            if (ammo > 0)
            {
                image.enabled = true;
                ammo--;
            }
            else { image.enabled = false; }
        }
    }
}
