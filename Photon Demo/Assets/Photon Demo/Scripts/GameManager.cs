using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public string PlayerPrefabName = "PhotonPlayer";
    public GameObject BulletPrefab;
    public AudioSource AudioSource;
    public AudioClip ShootClip;



    public static GameManager Instance;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;

    }

    private void OnLevelWasLoaded(int level)
    {
        PhotonNetwork.Instantiate(PlayerPrefabName, new Vector3(Random.Range(0, 5), 0, Random.Range(0, 5)),Quaternion.identity,0);
    }
}
