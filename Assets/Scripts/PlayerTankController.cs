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
    [SerializeField] private float maxRearSpeed = -300.0f;
    [SerializeField] private float attackSpeed = 0.5f;
    [SerializeField] private GameObject gameOverScreen;
    private float _currentSpeed, _targetSpeed;
    private float _timePassed;
    private float _health = 100;

    // Start is called before the first frame update
    private void Start()
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
        var playersPlane = new Plane(Vector3.up, transform.position + new Vector3(0, 0, 0));
        
        // Raycast
        var rayCast = //Camera.main.ScreenPointToRay(Input.mousePosition);
        UnityEngine.Camera.main.ScreenPointToRay(Input.mousePosition);
        
        // Distance
        if (playersPlane.Raycast(rayCast, out var impactDistance))
        {
            var impactPoint = rayCast.GetPoint(impactDistance);
            var targetRotation = Quaternion.LookRotation(impactPoint - transform.position);
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
        if (!Input.GetMouseButtonDown(0) || !(_timePassed >= attackSpeed)) return;
        // Time reset
        _timePassed = 0.0f;
        // Bullet instance
        bullet.tag = "Bullet";
        bullet.layer = LayerMask.NameToLayer("Player");
        Instantiate(bullet, bulletSpawnPoint.transform.position, bulletSpawnPoint.transform.rotation);
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("NPCBullet")) return;
        _health -= 20;
        if (!(_health <= 0)) return;
        Debug.Log("Player dead");
        gameOverScreen.SetActive(true);
        Time.timeScale = 0;
    }
}
