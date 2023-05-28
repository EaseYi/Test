using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;

public class PlayerShooting :NetworkBehaviour 
{
    private const string PLAYER_TAG = "Player";

    private WeaponManger weaponManger;
    private PlayerWeapon currentWeapon;

    private float shootCoolDownTime = 0f;//距离上次开枪时间
    private int autoShootCount = 0;//当前连开了多少枪

    private PlayerController playerController;

    [SerializeField]
    private LayerMask mask;
    private Camera cam;
    enum HitEffectMaterial
    {
        Metal,
        Stone,
    }
    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponentInChildren<Camera>();
        weaponManger= GetComponent<WeaponManger>();
        playerController= GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        shootCoolDownTime += Time.deltaTime;//记录开枪时间
        if (!IsLocalPlayer) return;
        currentWeapon = weaponManger.GetCurrentWeapom();

        /* if (Input.GetKeyDown(KeyCode.K))
         {
             ShootServerRpc(transform.name,10000);
         }*/    //测试

        if (Input.GetKeyDown(KeyCode.R))
        {
            weaponManger.Reload(currentWeapon);
        }
        if (currentWeapon.shootRate <= 0) //单发模式
        {
            if (Input.GetButtonDown("Fire1")&&shootCoolDownTime>=currentWeapon.shootCoolDownTime)
            {
                autoShootCount = 0;
                Shoot();
                shootCoolDownTime= 0f;//重置开枪时间；
            }
        }
        else
        {
            if (Input.GetButtonDown("Fire1"))                //按住
            {
                autoShootCount = 0;
                InvokeRepeating("Shoot", 0f, 1f / currentWeapon.shootRate);                //周期性地调动这个函数，第一个参数：多少秒后第一次调用； 第二个参数：平均每两次的执行间隔
            }
            else if (Input.GetButtonUp("Fire1") || Input.GetKeyDown(KeyCode.Q))
            { 
                CancelInvoke("Shoot");
            }
        }
    }

    public void StopShooting()
    {
        CancelInvoke("Shoot");
    }
    private void OnHit(Vector3 pos, Vector3 normal, HitEffectMaterial material)  // 击中点的特效
    {
        GameObject hitEffectPrefab;
        if (material == HitEffectMaterial.Metal)
        {
            hitEffectPrefab = weaponManger.GetCurrentGraphics().metalHitEffectPrefab;
        }
        else
        {
            hitEffectPrefab = weaponManger.GetCurrentGraphics().stoneHitEffectPrefab2;
        }

        GameObject hitEffectObject = Instantiate(hitEffectPrefab, pos, Quaternion.LookRotation(normal));//生成实例
        ParticleSystem particlesystem = hitEffectObject.GetComponent<ParticleSystem>();
        particlesystem.Emit(1); //立即触发
        particlesystem.Play();
        Destroy(hitEffectObject, 1f);//销毁实例
    }
    [ClientRpc]
    private void OnHitClientRpc(Vector3 pos, Vector3 normal, HitEffectMaterial material)
    {
        OnHit(pos, normal, material);
    }

    [ServerRpc]
    private void OnHitServerRpc(Vector3 pos, Vector3 normal, HitEffectMaterial material)
    {
        if (!IsHost)
        {
            OnHit(pos, normal, material);
        }
        OnHitClientRpc(pos, normal, material);
    }
    private void OnShoot(float recoilForce)
    {
        weaponManger.GetCurrentGraphics().muzzleFlash.Play();
        weaponManger.GetCurrentAudioSource().Play();

        if (IsLocalPlayer)//施加后坐力
        { 
            playerController.AddrecoilForce(recoilForce);
        }
    }
    [ClientRpc]
    private void OnShootClientRpc(float recoilForce)
    {
        OnShoot(recoilForce);
    }

    [ServerRpc]
    private void OnShootServerRpc(float recoilForce)
    {
        if (!IsHost)
        {
            OnShoot(recoilForce);
        }
        OnShootClientRpc(recoilForce);
    }
    private void Shoot()
    {
        if(currentWeapon.bullets <= 0 || currentWeapon.isReloading) return;

        currentWeapon.bullets--;

        if (currentWeapon.bullets <= 0)
        {
            weaponManger.Reload(currentWeapon);
        }

        autoShootCount++;
        autoShootCount++;
        float recoilForce = currentWeapon.recoilForce;
        if (autoShootCount <= 3)
        {
            recoilForce *= 0.2f;
        }
        OnShootServerRpc(recoilForce);
        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward,out hit,currentWeapon.range,mask))
        {
            if (hit.collider.tag == PLAYER_TAG)
            {
                ShootServerRpc(hit.collider.name, currentWeapon.damage);
                OnHitServerRpc(hit.point, hit.normal, HitEffectMaterial.Metal);
            }
            else
            {
                OnHitServerRpc(hit.point, hit.normal, HitEffectMaterial.Stone);
            }
        }
    }
    [ServerRpc]
    private void ShootServerRpc(string  name,int damage)
    {
        Player player = GameManager.Singleton.GetPlayer(name);
        player.TakeDamage(damage);
    }

}