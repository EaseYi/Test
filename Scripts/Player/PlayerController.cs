using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField]
    private Rigidbody rb;
    [SerializeField]
    private Camera cam;

    private Vector3 velocity = Vector3.zero; // 速度


    private Vector3 xRotation= Vector3.zero;// 旋转摄像机
    private Vector3 yRotation= Vector3.zero;//旋转视角
    private float cameraRotationTotal = 0f; //累计旋转的角度
    private float recoilForce = 0f; //后坐力；
    [SerializeField]
    private float cameraRptationLimit = 85f;//最大旋转角度

    private Vector3 thrusterForce = Vector3.zero;// 向上推力
    private float eps = 0.01f;
    private Vector3 lastFramePosition = Vector3.zero;  // 记录上一帧的位置
    private Animator animator;

    private float distansceToGround = 0f; //离地距离

    private void Start()
    {
        lastFramePosition = transform.position;
        animator = GetComponentInChildren<Animator>();
        distansceToGround = GetComponent<Collider>().bounds.extents.y;
    }

    public void Move(Vector3 _velocity)
    {
      velocity= _velocity;
    }
    public void Rotate(Vector3 _yRotation, Vector3 _xRotation)
    {
        yRotation = _yRotation;
        xRotation = _xRotation;
    }
    public void Thrust(Vector3 _thrusterForce)
    {
        thrusterForce = _thrusterForce;
    }

    public void AddrecoilForce(float newRecoilForce)
    { 
        recoilForce += newRecoilForce;
     }

    private void PerformMovement()
    {
        if(velocity!=Vector3.zero)
            rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
        if (thrusterForce != Vector3.zero)
        {
            rb.AddForce(thrusterForce);
            thrusterForce= Vector3.zero; 
        }   
    }
    private void PerformRotation()
    {
        if (recoilForce < 0.1)
        {
            recoilForce = 0;
        }
        if (yRotation != Vector3.zero||recoilForce>0)
        {
            rb.transform.Rotate(yRotation+rb.transform.up*Random.Range(-1f*recoilForce,1f*recoilForce));
        }
        if (xRotation != Vector3.zero||recoilForce>0)
        {
            cameraRotationTotal += xRotation.x-recoilForce;
            cameraRotationTotal = Mathf.Clamp(cameraRotationTotal,-cameraRptationLimit,cameraRptationLimit); //Mathf.Clamp(float value,float min,float max)
            cam.transform.localEulerAngles = new Vector3(cameraRotationTotal, 0f, 0f);
        }
        recoilForce *= 0.5f;//先快后慢
    }
    private void PerformAnimation()
    {
        // 计算坐标变化的值并更新上一帧的坐标
        Vector3 deltaPosition = transform.position - lastFramePosition;
        lastFramePosition = transform.position;

        //计算向量的八个方向，这里使用向量的点积运算
        float forward = Vector3.Dot(deltaPosition, transform.forward);
        float right = Vector3.Dot(deltaPosition, transform.right);

        int direction = 0; //静止

        if (forward > eps) //向前的情况
        {
            direction = 1; //向前移动
        }
        else if (forward < -eps) //向后退的情况
        {
            if (right > eps) //向右后方退
            {
                direction = 4;
            }
            else if (right < -eps) //向左后方退
            {
                direction = 6;
            }
            else //后退
            {
                direction = 5;
            }
        }
        else if (right > eps) //向右的情况
        {
            direction = 3; //向右移动
        }
        else if (right < -eps) //向左的情况
        {
            direction = 7;
        }

        if (!Physics.Raycast(transform.position, -Vector3.up, distansceToGround + 0.1f))
        {
            direction = 8;
        }
        if (GetComponent<Player>().IsDead())
        {
            direction = -1;
        }

            animator.SetInteger("direction", direction);

    }
    private void FixedUpdate()
    {
        if (IsLocalPlayer)
        {
            PerformMovement();
            PerformRotation();
        }
        if (IsLocalPlayer)
        {
            PerformAnimation();
        }
    }
    private void Update()
    {
        if (!IsLocalPlayer)
        {
            PerformAnimation();
        }
    }
}
