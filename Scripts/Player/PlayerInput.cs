using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [SerializeField]
    private float speed = 5f;
    [SerializeField]
    private PlayerController controller;
    [SerializeField]
    private float lookSensitivity = 5f;
    [SerializeField]
    private float thrusterForce = 20f;
    
    private float distansceToGround=0f;
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        distansceToGround = GetComponent<Collider>().bounds.extents.y;
    }

    // Update is called once per frame
    void Update()
    {
        // wasd“∆∂Ø
        float xMov = Input.GetAxisRaw("Horizontal");
        float yMov = Input.GetAxisRaw("Vertical");

        Vector3 velocity = (transform.right * xMov + transform.forward * yMov).normalized * speed;
        controller.Move(velocity);

        // ”Ω«
        float xMouse = Input.GetAxisRaw("Mouse X");
        float yMouse = Input.GetAxisRaw("Mouse Y");

        Vector3 xRotation = new Vector3(-yMouse, 0f, 0f) * lookSensitivity;
        Vector3 yRotation = new Vector3(0, xMouse, 0f) * lookSensitivity;
        controller.Rotate(yRotation, xRotation);

        //ø’∏ÒÃ¯‘æ
        if (Input.GetButton("Jump"))     
        {
            if (Physics.Raycast(transform.position, -Vector3.up, distansceToGround + 0.1f))
            {
                Vector3 force = Vector3.up * thrusterForce;
                controller.Thrust(force);
            }
        }
    }
}
