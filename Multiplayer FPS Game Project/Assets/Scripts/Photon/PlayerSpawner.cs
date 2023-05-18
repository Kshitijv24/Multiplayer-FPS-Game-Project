using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerSpawner : MonoBehaviour
{
    public static PlayerSpawner Instance;

    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject deathEffect;
    [SerializeField] float respawnTime;

    GameObject player;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            SpawnPlayer();
        }
    }

    public void SpawnPlayer()
    {
        Transform spawnPoint = SpawnPointManager.Instance.GetSpawnPoint();

        player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, spawnPoint.rotation);
    }

    public void Die(string damager)
    {
        UIController.Instance.deathText.text = "You were killed by " + damager;

        //PhotonNetwork.Destroy(player);
        //SpawnPlayer();

        MatchManager.Instance.UpdateStatsSend(PhotonNetwork.LocalPlayer.ActorNumber, 1, 1);

        if(player != null)
        {
            StartCoroutine(DieCoroutine());
        }
    }

    public IEnumerator DieCoroutine()
    {
        PhotonNetwork.Instantiate(deathEffect.name, player.transform.position, Quaternion.identity);
        PhotonNetwork.Destroy(player);

        UIController.Instance.deathScreen.SetActive(true);
        yield return new WaitForSeconds(respawnTime);
        UIController.Instance.deathScreen.SetActive(false);

        SpawnPlayer();
    }
}
