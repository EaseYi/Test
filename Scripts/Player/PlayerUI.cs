using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    public static PlayerUI Singleton;

    private Player player=null;

    [SerializeField]
    private TextMeshProUGUI bulletsText;
    [SerializeField]
    private GameObject bulletsObeject;

    private WeaponManger weaponManger;

    [SerializeField]
    private Transform healthBarFill;
    [SerializeField]
    private GameObject healthBarObeject;

    private void Awake()
    {
        Singleton = this;
    }

    public void setPlayer(Player localPlayer)
    {
        player = localPlayer;
        weaponManger=player.GetComponent<WeaponManger>(); 
        bulletsObeject.SetActive(true); 
        healthBarObeject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null) return;

        var currentWeapon = weaponManger.GetCurrentWeapom();
        if (currentWeapon.isReloading)
        {
            bulletsText.text = "Reloading...";
        }
        else
        {
            bulletsText.text = "Bullets: " + currentWeapon.bullets + "/" + currentWeapon.maxBullets;
        }

        healthBarFill.localScale = new Vector3(player.GetHealth() / 100f, 1f, 1f);
    }
}
