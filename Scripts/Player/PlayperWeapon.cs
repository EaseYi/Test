using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class PlayerWeapon
{
    public string name = "Uzi";
    public int damage = 10;
    public float range = 100f;
    public float shootRate = 10f;//射击频率
    public float shootCoolDownTime = 0.75f; //单发冷却时间
    public float recoilForce = 2f; //后坐力
    public int maxBullets = 30;
    public int bullets = 30;
    public float reloadTime = 2f;

    [HideInInspector]
    public bool isReloading = false;

    public GameObject graphics;
}
