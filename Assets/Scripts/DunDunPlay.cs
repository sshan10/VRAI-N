using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DunDunPlay : MonoBehaviour
{
    public GameObject player;
    public GameObject DunDunPrefab;
    public GameObject Instance = null;
    public Transform playerTransform;

    void Awake()
    {
        playerTransform = Camera.main.transform;
    }

    public void Instantiate()
    {
        Vector3 spawnPosition = GetSpawnPosition();

        if(Instance == null)
        {
            Instance = Instantiate(DunDunPrefab);            
        }

        Instance.transform.position = spawnPosition;
        Instance.transform.LookAt(playerTransform);
    }

    private Vector3 GetSpawnPosition()
    {
        Vector3 offset = playerTransform.forward.normalized * 3;
        Vector3 offsetPosition = playerTransform.position + offset;

        return offsetPosition;
    }
}
