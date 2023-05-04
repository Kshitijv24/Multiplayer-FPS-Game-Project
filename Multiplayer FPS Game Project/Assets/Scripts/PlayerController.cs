using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] Transform viewPoint;
    [SerializeField] float mouseSensitivity = 1f;
    [SerializeField] float moveSpeed = 1f;

    float mouseVerticalRotation;
    Vector2 mouseInput;
    Vector3 moveDirection, movement;

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

        movement = ((transform.forward * moveDirection.z) + (transform.right * moveDirection.x)).normalized;

        transform.position += movement * moveSpeed * Time.deltaTime;
    }
}
