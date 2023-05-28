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

    private float shootCoolDownTime = 0f;//�����ϴο�ǹʱ��
    private int autoShootCount = 0;//��ǰ�����˶���ǹ

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
        shootCoolDownTime += Time.deltaTime;//��¼��ǹʱ��
        if (!IsLocalPlayer) return;
        currentWeapon = weaponManger.GetCurrentWeapom();

        /* if (Input.GetKeyDown(KeyCode.K))
         {
             ShootServerRpc(transform.name,10000);
         }*/    //����

        if (Input.GetKeyDown(KeyCode.R))
        {
            weaponManger.Reload(currentWeapon);
        }
        if (currentWeapon.shootRate <= 0) //����ģʽ
        {
            if (Input.GetButtonDown("Fire1")&&shootCoolDownTime>=currentWeapon.shootCoolDownTime)
            {
                autoShootCount = 0;
                Shoot();
                shootCoolDownTime= 0f;//���ÿ�ǹʱ�䣻
            }
        }
        else
        {
            if (Input.GetButtonDown("Fire1"))                //��ס
            {
                autoShootCount = 0;
                InvokeRepeating("Shoot", 0f, 1f / currentWeapon.shootRate);                //�����Եص��������������һ����������������һ�ε��ã� �ڶ���������ƽ��ÿ���ε�ִ�м��
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
    private void OnHit(Vector3 pos, Vector3 normal, HitEffectMaterial material)  // ���е����Ч
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

        GameObject hitEffectObject = Instantiate(hitEffectPrefab, pos, Quaternion.LookRotation(normal));//����ʵ��
        ParticleSystem particlesystem = hitEffectObject.GetComponent<ParticleSystem>();
        particlesystem.Emit(1); //��������
        particlesystem.Play();
        Destroy(hitEffectObject, 1f);//����ʵ��
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

        if (IsLocalPlayer)//ʩ�Ӻ�����
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