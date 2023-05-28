using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerSetup : NetworkBehaviour
{
    [SerializeField]
    private Behaviour[] componentsToDisable;

    private Camera SceneCamera;

    // Start is called before the first frame update
    public override void OnNetworkSpawn()
    { 
        //base.OnNetworkSpawn();
        if (!IsLocalPlayer)
        {
            SetLayerMaskForAllChildren(transform, LayerMask.NameToLayer("Romote Player"));
            DisableComponents();
        }
        else
        {
            PlayerUI.Singleton.setPlayer(GetComponent<Player>()); 
            SetLayerMaskForAllChildren(transform, LayerMask.NameToLayer("Player"));
            SceneCamera = Camera.main;
            if (SceneCamera != null)
            {
                SceneCamera.gameObject.SetActive(false); //将主摄像机改为非活跃状态
            }
        }
        string name = "Player " + GetComponent<NetworkObject>().NetworkObjectId.ToString();
        Player player = GetComponent<Player>();
        player.Setup();

        GameManager.Singleton.RegisterPlayer(name, player);
    }
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (SceneCamera != null)
        {
            SceneCamera.gameObject.SetActive(true);
        }
        GameManager.Singleton.UnregisterPlayer(transform.name);
    }

    private void SetLayerMaskForAllChildren(Transform transform, LayerMask layerMask)
    { 
        transform.gameObject.layer= layerMask;
        for (int i = 0; i < transform.childCount; i++)
        {
            SetLayerMaskForAllChildren(transform.GetChild(i), layerMask); 
        }
    }


    private void DisableComponents()
    {
        for (int i = 0; i < componentsToDisable.Length; i++)
            componentsToDisable[i].enabled = false; //禁用掉所以的除去自己以外的组件
    }


}
