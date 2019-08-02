﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DunDunPlay : MonoBehaviour
{
    public GameObject DunDunPrefab;
    public GameObject Instance = null;
    public Transform playerTransform;

    private Animator DunDunAnimatorController;

    void Awake()
    {
        playerTransform = Camera.main.transform;
    }

    public void Instantiate()
    {
        Vector3 spawnPosition = GetNextPosition();

        if (Instance == null)
        {
            Instance = Instantiate(DunDunPrefab);
        }

        Instance.transform.position = spawnPosition;
        Instance.transform.LookAt(playerTransform);

        DunDunAnimatorController = Instance.GetComponent<Animator>();
    }

    private Vector3 GetNextPosition()
    {
        Vector3 offset = playerTransform.forward.normalized * 3;
        Vector3 offsetPosition = playerTransform.position + offset;

        return offsetPosition;
    }

    public void transPosition()
    {
        Vector3 nextPosition = GetNextPosition();

        Instance.transform.position = nextPosition;
        DunDunAnimatorController.SetTrigger("Activate");        
    }

    public void Unforseen()
    {
        DunDunAnimatorController.SetTrigger("Deactivate");
    }
}