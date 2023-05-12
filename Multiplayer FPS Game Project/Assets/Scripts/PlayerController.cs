using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviourPunCallbacks
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

    [Header("================= Bullet Variables =================")]
    [Space(10)]
    [SerializeField] GameObject bulletImpact;
    //[SerializeField] float timeBetweenShots;
    [SerializeField] GameObject playerHitImpact;

    [Header("================= Gun Variables=================")]
    [Space(10)]
    [SerializeField] float maxHeat;
    //[SerializeField] float heatPerShot;
    [SerializeField] float coolRate;
    [SerializeField] float overheatCoolRate;
    [SerializeField] Gun[] gunArray;
    [SerializeField] float muzzleDisplayTime;

    float mouseVerticalRotation;
    Vector2 mouseInput;
    Vector3 moveDirection;
    Vector3 movement;
    float activeMoveSpeed;
    bool isGrounded;
    Camera mainCamera;
    float shotCounter;
    float heatCounter;
    bool overHeated;
    int selectedGun;
    float muzzleCounter;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        mainCamera = Camera.main;
        UIController.Instance.weaponTemperatureSlider.maxValue = maxHeat;
        SwitchGun();

        //Transform newSpawnPoint = SpawnPointManager.Instance.GetSpawnPoint();
        //transform.position = newSpawnPoint.position;
        //transform.rotation = newSpawnPoint.rotation;
    }

    private void Update()
    {
        if (photonView.IsMine)
        {
            HandlePlayerMouseMovement();
            HandlePlayerMovement();
            HandlePlayerShooting();
        }
    }

    private void LateUpdate()
    {
        if (photonView.IsMine)
        {
            mainCamera.transform.position = viewPoint.position;
            mainCamera.transform.rotation = viewPoint.rotation;
        }
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

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else if(Cursor.lockState == CursorLockMode.None)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
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

    private void HandlePlayerShooting()
    {
        if (gunArray[selectedGun].muzzleFlash.activeInHierarchy)
        {
            muzzleCounter -= Time.deltaTime;

            if(muzzleCounter <= 0)
            {
                gunArray[selectedGun].muzzleFlash.SetActive(false);
            }
        }

        if (!overHeated)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Shoot();
            }

            if (Input.GetMouseButton(0) && gunArray[selectedGun].isAutomatic)
            {
                shotCounter -= Time.deltaTime;

                if (shotCounter <= 0)
                {
                    Shoot();
                }
            }

            heatCounter -= coolRate * Time.deltaTime;
        }
        if(heatCounter <= 0)
        {
            heatCounter = 0f;
        }
        else
        {
            heatCounter -= overheatCoolRate * Time.deltaTime;

            if(heatCounter <= 0 )
            {
                overHeated = false;
                UIController.Instance.overheatedMessage.gameObject.SetActive(false);
            }
        }
        UIController.Instance.weaponTemperatureSlider.value = heatCounter;

        if(Input.GetAxisRaw("Mouse ScrollWheel") >  0)
        {
            selectedGun++;

            if(selectedGun >= gunArray.Length)
            {
                selectedGun = 0;
            }
            SwitchGun();
        }
        else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0)
        {
            selectedGun--;

            if (selectedGun < 0)
            {
                selectedGun = gunArray.Length - 1;
            }
            SwitchGun();
        }
        for (int i = 0; i < gunArray.Length; i++)
        {
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                selectedGun = i;
                SwitchGun();
            }
        }
    }

    private void Shoot()
    {
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        ray.origin = mainCamera.transform.position;

        if(Physics.Raycast(ray, out RaycastHit hit))
        {
            if(hit.collider.gameObject.tag == "Player")
            {
                Debug.Log("Hit " + hit.collider.gameObject.GetPhotonView().Owner.NickName);
                PhotonNetwork.Instantiate(playerHitImpact.name, hit.point, Quaternion.identity);

                hit.collider.gameObject.GetPhotonView().RPC(nameof(DealDamage), RpcTarget.All, photonView.Owner.NickName);
            }
            else
            {
                GameObject bulletImpactObject = Instantiate(
                bulletImpact, hit.point + (hit.normal * 0.002f),
                Quaternion.LookRotation(hit.normal,
                Vector3.up));

                Destroy(bulletImpactObject, 1f);
            }
        }

        shotCounter = gunArray[selectedGun].timeBetweenShots;

        heatCounter += gunArray[selectedGun].heatPerShot;

        if(heatCounter >= maxHeat)
        {
            heatCounter = maxHeat;
            overHeated = true;
            UIController.Instance.overheatedMessage.gameObject.SetActive(true);
        }
        gunArray[selectedGun].muzzleFlash.SetActive(true);
        muzzleCounter = muzzleDisplayTime;
    }

    private void SwitchGun()
    {
        foreach(Gun guns in gunArray)
        {
            guns.gameObject.SetActive(false);
        }

        gunArray[selectedGun].gameObject.SetActive(true);
        gunArray[selectedGun].muzzleFlash.SetActive(false);
    }

    [PunRPC]
    public void DealDamage(string damager)
    {
        TakeDamage(damager);
    }

    public void TakeDamage(string damager)
    {
        if (photonView.IsMine)
        {
            //Debug.Log(photonView.Owner.NickName + "has been hit by " + damager);

            PlayerSpawner.Instance.Die();
        }
    }
}
