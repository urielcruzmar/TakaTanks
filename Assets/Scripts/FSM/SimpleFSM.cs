using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class SimpleFSM : FSM
{
    public enum FSMState
    {
        None, Patrol, Chase, Attack, Dead
    }

    // NPC current state
    public FSMState currentState = FSMState.Patrol;
    
    // Tank speed and rotation speed
    private float _currentSpeed = 150.0f;
    private float _currentRotationSpeed = 2.0f;
    
    // Bullet
    public GameObject bullet;
    
    // NPC life
    private bool _tDead = false;
    private int _health = 100;
    private Rigidbody _rigidbody;
    protected Transform PlayerTransform;
    
    // Next destination
    protected Vector3 DestinationPosition;
    
    // Patrol points list
    protected GameObject[] PointList;
    
    // Bullet shooting speed
    protected float ShootRate = 3.0f;
    protected float ElapsedTime = 0.0f;
    public float maxFireAimError = 0.0001f;
    
    // Radius values
    public float patrollingRadius = 100.0f;
    public float attackRadius = 200.0f;
    public float playerNearRadius = 300.0f;
    
    // Tank turret
    public Transform turret;
    public Transform bulletSpawnPoint;

    protected override void Initialize()
    {
        // Get point list
        PointList = GameObject.FindGameObjectsWithTag("WanderPoint");
        
        // Get next point
        FindNextPoint();
        
        // Get enemy (player)
        GameObject objPlayer = GameObject.FindGameObjectWithTag("Player");
        _rigidbody = GetComponent<Rigidbody>();
        PlayerTransform = objPlayer.transform;
        if (!PlayerTransform)
        {
            print("Player does not exist");
        }
    }

    protected override void FSMUpdate()
    {
        switch (currentState)
        {
            case FSMState.Patrol:
                UpdatePatrolState();
                break;
            case FSMState.Chase:
                UpdateChaseState();
                break;
            case FSMState.Attack:
                UpdateAttackState();
                break;
            case FSMState.Dead:
                UpdateDeadState();
                break;
            case FSMState.None:
                print("State not set");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        ElapsedTime += Time.deltaTime;

        if (_health <= 0)
        {
            currentState = FSMState.Dead;
        }
    }

    protected void UpdatePatrolState()
    {
        // Search new point if reached
        if (Vector3.Distance(transform.position, DestinationPosition) <= patrollingRadius)
        {
            print("Reached patrol point, searching next point");
            FindNextPoint();
        }
        
        // Check player distance
        else if (Vector3.Distance(transform.position, PlayerTransform.position) <= playerNearRadius)
        {
            print("Chasing player");
            currentState = FSMState.Chase;
        }
        
        // Rotating to player
        Quaternion targetRotation = Quaternion.LookRotation((DestinationPosition - transform.position));
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _currentRotationSpeed);
        
        // Forward
        transform.Translate(Vector3.forward * (Time.deltaTime * _currentSpeed));
    }
    
    protected void UpdateChaseState()
    {
        var position = PlayerTransform.position;
        DestinationPosition = position;
        float dist = Vector3.Distance(transform.position, position);

        // Check distance to player and update state
        if (dist <= attackRadius)
        {
            currentState = FSMState.Attack;
        }
        else if (dist >= playerNearRadius)
        {
            currentState = FSMState.Patrol;
        }
        
        // Forward
        transform.Translate(Vector3.forward * (Time.deltaTime * _currentSpeed));
    }
    
    protected void UpdateAttackState()
    {
        var position = PlayerTransform.position;
        DestinationPosition = position;
        Vector3 frontVector = Vector3.forward;

        float dist = Vector3.Distance(transform.position, position);
        if (dist >= attackRadius && dist < playerNearRadius)
        {
            // Rotate to target
            Quaternion targetRotation =
                Quaternion.FromToRotation(frontVector, DestinationPosition - transform.position);
            transform.rotation =
                Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _currentRotationSpeed);
            
            // Forward
            transform.Translate(Vector3.forward * (Time.deltaTime * _currentSpeed));
            
            // Update state
            currentState = FSMState.Attack;
        }
        // Check distance
        else if (dist >= playerNearRadius)
        {
            // Update state
            currentState = FSMState.Patrol;
        }
        
        // Rotate turret to player
        Quaternion turretRotation = Quaternion.FromToRotation(frontVector, DestinationPosition - transform.position);
        turret.rotation = Quaternion.Slerp(turret.rotation, turretRotation, Time.deltaTime * _currentRotationSpeed);
        
        // Shoot bullets
        if (Mathf.Abs(Quaternion.Dot(turretRotation, turret.rotation)) > 1.0f - maxFireAimError)
        {
            ShootBullet();
        }
    }
    
    protected void UpdateDeadState()
    {
        if (!_tDead)
        {
            _tDead = true;
            Explode();
        }
    }
    
    protected void FindNextPoint()
    {
        print("Searching for new point");
        int randomIndex = Random.Range(0, PointList.Length);
        float randomRadius = 10.0f;

        Vector3 randomPosition = Vector3.zero;
        DestinationPosition = PointList[randomIndex].transform.position + randomPosition;
        
        // Check valid point
        if (IsInCurrentRange(DestinationPosition))
        {
            randomPosition = new Vector3(Random.Range(-randomRadius, randomRadius), 0.0f, Random.Range(-randomRadius, randomRadius));
            DestinationPosition = PointList[randomIndex].transform.position + randomPosition;
        }
    }

    protected bool IsInCurrentRange(Vector3 position)
    {
        var position1 = transform.position;
        float xPos = Mathf.Abs(position.x - position1.x);
        float zPos = Mathf.Abs(position.z - position1.z);

        return xPos <= 50.0f && zPos <= 50.0f;
    }

    private void ShootBullet()
    {
        if (ElapsedTime >= ShootRate)
        {
            Instantiate(bullet, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
            ElapsedTime = 0.0f;
        }
    }

    protected void Explode()
    {
        float randomX = Random.Range(10.0f, 30.0f);
        float randomZ = Random.Range(10.0f, 30.0f);
        for (int i = 0; i < 3; i++)
        {
            _rigidbody.AddExplosionForce(10000.0f, transform.position - new Vector3(randomX, 10.0f, randomZ), 40.0f, 10.0f);
            _rigidbody.velocity = transform.TransformDirection(new Vector3(randomX, 20.0f, randomZ));
        }
        Destroy(gameObject, 1.5f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            _health -= collision.gameObject.GetComponent<Bullet>().damage;
        }
    }
}
