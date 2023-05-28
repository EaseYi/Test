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

    private Vector3 velocity = Vector3.zero; // �ٶ�


    private Vector3 xRotation= Vector3.zero;// ��ת�����
    private Vector3 yRotation= Vector3.zero;//��ת�ӽ�
    private float cameraRotationTotal = 0f; //�ۼ���ת�ĽǶ�
    private float recoilForce = 0f; //��������
    [SerializeField]
    private float cameraRptationLimit = 85f;//�����ת�Ƕ�

    private Vector3 thrusterForce = Vector3.zero;// ��������
    private float eps = 0.01f;
    private Vector3 lastFramePosition = Vector3.zero;  // ��¼��һ֡��λ��
    private Animator animator;

    private float distansceToGround = 0f; //��ؾ���

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
        recoilForce *= 0.5f;//�ȿ����
    }
    private void PerformAnimation()
    {
        // ��������仯��ֵ��������һ֡������
        Vector3 deltaPosition = transform.position - lastFramePosition;
        lastFramePosition = transform.position;

        //���������İ˸���������ʹ�������ĵ������
        float forward = Vector3.Dot(deltaPosition, transform.forward);
        float right = Vector3.Dot(deltaPosition, transform.right);

        int direction = 0; //��ֹ

        if (forward > eps) //��ǰ�����
        {
            direction = 1; //��ǰ�ƶ�
        }
        else if (forward < -eps) //����˵����
        {
            if (right > eps) //���Һ���
            {
                direction = 4;
            }
            else if (right < -eps) //�������
            {
                direction = 6;
            }
            else //����
            {
                direction = 5;
            }
        }
        else if (right > eps) //���ҵ����
        {
            direction = 3; //�����ƶ�
        }
        else if (right < -eps) //��������
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
