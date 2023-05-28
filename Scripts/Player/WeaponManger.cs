using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class WeaponManger : NetworkBehaviour
{
    [SerializeField]
    private PlayerWeapon primaryWeapon;
    [SerializeField]
    private PlayerWeapon secondaryWeapon;
    [SerializeField]
    private GameObject weaponHolder;
    
    private PlayerWeapon currentWeapom;
    private WeaponGraphics currentGraphics;
    private AudioSource currentAudioSource;
    // Start is called before the first frame update
    void Start()
    {
        EquipWeapon(primaryWeapon); 
    }
    public void EquipWeapon(PlayerWeapon weapon)
    { 
        currentWeapom= weapon;
        if (weaponHolder.transform.childCount > 0)
        {
            Destroy(weaponHolder.transform.GetChild(0).gameObject);
        }
        GameObject weaponObject = Instantiate(currentWeapom.graphics, weaponHolder.transform.position, weaponHolder.transform.rotation);
        weaponObject.transform.SetParent(weaponHolder.transform);

        currentGraphics= weaponObject.GetComponent<WeaponGraphics>();
        currentAudioSource= weaponObject.GetComponent<AudioSource>();
        if (IsLocalPlayer)
        {
            currentAudioSource.spatialBlend = 0f; //自己开枪音效修改为2d
         }
    }
    public PlayerWeapon GetCurrentWeapom()
    { 
        return currentWeapom;
    }
    public WeaponGraphics GetCurrentGraphics()
    {
        return currentGraphics;
    }
    public AudioSource GetCurrentAudioSource()
    {
        return currentAudioSource;
    }

    // Update is called once per frame
    public void ToggleWeapon()
    {

        if (currentWeapom == primaryWeapon)
        {
            Debug.Log("primary");
            EquipWeapon(secondaryWeapon);
        }
        else
        {
            Debug.Log("second");
            EquipWeapon(primaryWeapon);
        }
    }

    [ClientRpc]
    private void ToggleWeaponClientRpc()
    {
        ToggleWeapon();
    }

    [ServerRpc]
    private void ToggleWeaponServerRpc()
    {
        //ToggleWeapon(); 如果开启host来调试的话，则需要注释掉，不然的话会发生错误，枪会套枪
        if (!IsHost)
        { 
            ToggleWeapon();
        }
        ToggleWeaponClientRpc();
    }
    void Update()
    {
        if (IsLocalPlayer)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                ToggleWeaponServerRpc();
            }
        }
    }
    public void Reload(PlayerWeapon playerWeapon)
    {
        if (playerWeapon.isReloading) return;
        playerWeapon.isReloading = true;

        StartCoroutine(ReloadCoroutine(playerWeapon));
    }

    private IEnumerator ReloadCoroutine(PlayerWeapon playerWeapon)
    {
        yield return new WaitForSeconds(playerWeapon.reloadTime);

        playerWeapon.bullets = playerWeapon.maxBullets;
        playerWeapon.isReloading = false;
    }
}
