using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerTankController : MonoBehaviour
{
    [SerializeField] private GameObject turret;
    [SerializeField] private GameObject bullet;
    [SerializeField] private GameObject bulletSpawnPoint;
    
    [SerializeField] private float rotationSpeed = 150.0f;
    [SerializeField] private float turretRotationSpeed = 10.0f;
    [SerializeField] private float maxFrontSpeed = 300.0f;
    [SerializeField] private float maxRearSpeed = 300.0f;
    [SerializeField] private float attackSpeed = 0.5f;
    private float _currentSpeed, _targetSpeed;
    private float _timePassed;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateWeapon();
        UpdateControl();
    }

    private void UpdateControl()
    {
        // Mouse pointing
        // Upwards plante to intersect player
        Plane playersPlane = new Plane(Vector3.up, transform.position + new Vector3(0, 0, 0));
        
        // Raycast
        Ray rayCast = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        // Distance
        float impactDistance = 0;

        if (playersPlane.Raycast(rayCast, out impactDistance))
        {
            Vector3 impactPoint = rayCast.GetPoint(impactDistance);
            Quaternion targetRotation = Quaternion.LookRotation(impactPoint - transform.position);
            turret.transform.rotation = Quaternion.Slerp(turret.transform.rotation, targetRotation, Time.deltaTime * turretRotationSpeed);
        }
        
        // WS Movement
        if (Input.GetKey(KeyCode.W))
        {
            _targetSpeed = maxFrontSpeed;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            _targetSpeed = maxRearSpeed;
        }
        else
        {
            _targetSpeed = 0.0f;
        }
        
        // AD Movement
        if (Input.GetKey(KeyCode.A))
        {
            transform.Rotate(0, -rotationSpeed * Time.deltaTime, 0.0f);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            transform.Rotate(0, rotationSpeed * Time.deltaTime, 0.0f);
        }
        
        // Current speed
        _currentSpeed = Mathf.Lerp(_currentSpeed, _targetSpeed, 7.0f * Time.deltaTime);
        transform.Translate(Vector3.forward * (Time.deltaTime * _currentSpeed));
    }

    private void UpdateWeapon()
    {
        _timePassed += Time.deltaTime;

        if (Input.GetMouseButtonDown(0) && _timePassed >= attackSpeed)
        {
            // Time reset
            _timePassed = 0.0f;
            // Bullet instance
            Instantiate(bullet, bulletSpawnPoint.transform.position, bulletSpawnPoint.transform.rotation);
        }
    }
}
