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

    float mouseVerticalRotation;
    Vector2 mouseInput;
    Vector3 moveDirection, movement;
    float activeMoveSpeed;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        HandlePlayerMouseMovement();
        HandlePlayerMovement();
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

        movement = ((transform.forward * moveDirection.z) + (transform.right * moveDirection.x)).normalized * activeMoveSpeed;

        characterController.Move(movement * Time.deltaTime);
    }
}
