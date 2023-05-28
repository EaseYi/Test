using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SerializeField]
    private int maxHealth = 100;
    [SerializeField]
    private Behaviour[] componentsToDisable;
    private bool[] componentsEnabled;
    private bool colliderEnabled;

    
    private NetworkVariable<int> currentHealth = new NetworkVariable<int>();
    private NetworkVariable<bool> isDead = new NetworkVariable<bool>(); 

    public void Setup()
    {
        componentsEnabled= new bool[componentsToDisable.Length];
        for (int i = 0; i < componentsToDisable.Length; i++)
        {
            componentsEnabled[i] = componentsToDisable[i].enabled;
        }
        Collider col = GetComponent<Collider>();
        colliderEnabled = col.enabled;
        SetDefaults();
    }

    private void SetDefaults()
    {
        for (int i = 0; i < componentsToDisable.Length; i++)
        {
            componentsToDisable[i].enabled= componentsEnabled[i] ;
        }
        Collider col = GetComponent<Collider>();
        col.enabled = colliderEnabled;

        if (IsServer)
        {
            currentHealth.Value = maxHealth;
            isDead.Value = false;
        }
    }
    public bool IsDead()
    {
        return isDead.Value;
    }

    public void TakeDamage(int damage) //�ܵ��˺� ֻ�ڷ������˵���
    { 
        if (isDead.Value)
        {
            return;
        }
        currentHealth.Value -= damage;
        if (currentHealth.Value <=0)
        { 
            currentHealth.Value = 0;
            isDead.Value = true;
            if (!IsHost)
            {
                DieOnServer();
            }
            DieClidentRpc();
        }
    }
    private IEnumerator Respawn()   //����
    {
        yield return new WaitForSeconds(GameManager.Singleton.MatchingSetting.respawnTime);

        SetDefaults();
        GetComponentInChildren<Animator>().SetInteger("direction", 0);
        GetComponent<Rigidbody>().useGravity = true;
        if (IsLocalPlayer)
        {
            transform.position = new Vector3(0f, 10f, 0f);
        }
    }
    private void DieOnServer()
    {
        Die();
    }
    private void DieClidentRpc()
    {
        Die();
    }
    public void Die()  //����
    {
        GetComponent<PlayerShooting>().StopShooting();
        GetComponentInChildren<Animator>().SetInteger("direction", -1);
        GetComponent<Rigidbody>().useGravity = false;

        for (int i = 0; i < componentsToDisable.Length; i++)
        {
            componentsToDisable[i].enabled = false;
        }
        Collider col = GetComponent<Collider>();
        col.enabled = false;
        StartCoroutine(Respawn()); 
    }

    public int GetHealth()
    {
        return currentHealth.Value;    
    }
}
