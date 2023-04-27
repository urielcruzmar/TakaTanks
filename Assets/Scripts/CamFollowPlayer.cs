using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class CamFollowPlayer : MonoBehaviour
{
    [SerializeField] private Vector3 position = new Vector3(0, 10, -10);
    [SerializeField] private Vector3 rotation = new Vector3(30, 0, 0);
    public Transform player;

    private void Start()
    {
        transform.Rotate(rotation);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = player.transform.position + position;
    }
}
