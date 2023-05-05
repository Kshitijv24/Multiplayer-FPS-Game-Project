using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] Transform viewPoint;
    [SerializeField] float mouseSensitivity;
    [SerializeField] float moveSpeed;
    [SerializeField] CharacterController characterController;
    [SerializeField] float runSpeed;
    [SerializeField] float jumpForce;
    [SerializeField] float gravityMod;
    [SerializeField] Transform groundCheckPoint;
    [SerializeField] LayerMask groundLayer;

    float mouseVerticalRotation;
    Vector2 mouseInput;
    Vector3 moveDirection;
    Vector3 movement;
    float activeMoveSpeed;
    bool isGrounded;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        HandlePlayerMouseMovement();
        HandlePlayerMovement();
    }

    private void LateUpdate()
    {
        Camera.main.transform.position = viewPoint.position;
        Camera.main.transform.rotation = viewPoint.rotation;
    }

    private void HandlePlayerMouseMovement()
    {
        mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * mouseSensitivity;

        transform.rotation = Quaternion.Euler(
            transform.rotation.eulerAngles.x,
            transform.rotation.eulerAngles.y + mouseInput.x,
            transform.rotation.eulerAngles.z);

        mouseVerticalRotation += mouseInput.y;
        mouseVerticalRotation = Mathf.Clamp(mouseVerticalRotation, -60f, 60f);

        viewPoint.rotation = Quaternion.Euler(
            -mouseVerticalRotation,
            viewPoint.rotation.eulerAngles.y,
            transform.rotation.eulerAngles.z);
    }

    private void HandlePlayerMovement()
    {
        moveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));

        if (Input.GetKey(KeyCode.LeftShift))
        {
            activeMoveSpeed = runSpeed;
        }
        else
        {
            activeMoveSpeed = moveSpeed;
        }

        float yVelocity = movement.y;
        movement = ((transform.forward * moveDirection.z) + (transform.right * moveDirection.x)).normalized * activeMoveSpeed;
        movement.y = yVelocity;

        if(characterController.isGrounded)
            movement.y = 0f;

        isGrounded = Physics.Raycast(groundCheckPoint.position, Vector3.down, 0.25f, groundLayer);

        if (Input.GetButtonDown("Jump") && isGrounded)
            movement.y = jumpForce;

        movement.y += Physics.gravity.y * Time.deltaTime * gravityMod;
        characterController.Move(movement * Time.deltaTime);
    }
}
