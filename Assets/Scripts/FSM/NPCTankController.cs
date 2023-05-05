using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class NPCTankController : AdvanceFSM
{
    [SerializeField] private GameObject tankNPC;
    
    public GameObject bullet;
    public int health;

    private Rigidbody _rigidbody;
    private Transform _playerTransform;
    // Points
    protected Vector3 NextPosition;
    private GameObject[] _pointList;
    private GameObject[] _retreatList;
    // Shooting
    private float _shootRate;
    private float _elapsedTime;
    // Radius
    public float patrollingRadius = 100.0f;
    public float attackRadius = 200.0f;
    public float playerNearRadius = 300.0f;
    // Turret
    public Transform turret;
    public Transform bulletSpawnPoint;

    // FSM Initialize
    protected override void Initialize()
    {
        health = 100;
        _elapsedTime = 0.0f;
        _shootRate = 2.0f;
        // Get enemy
        var objPlayer = GameObject.FindGameObjectWithTag("Player");
        _playerTransform = objPlayer.transform;
        // Rigidbody
        _rigidbody = GetComponent<Rigidbody>();
        if (!_playerTransform)
        {
            Debug.LogError("NPCTankController ERROR: Player does not exist, add one with tag");
        }
        // Turret
        turret = gameObject.transform.GetChild(0).transform;
        bulletSpawnPoint = turret.GetChild(0).transform;
        // Start FSM
        ConstructFSM();
    }

    protected override void FSMUpdate()
    {
        _elapsedTime += Time.deltaTime;
    }

    protected override void FSMFixedUpdate()
    {
        CurrentState.CheckTransitionRules(_playerTransform, gameObject);
        CurrentState.RunState(_playerTransform, gameObject);
    }

    public void SetTransition(Transition transition)
    {
        PerformTransition(transition);
    }

    private void ConstructFSM()
    {
        // Waypoints list
        _pointList = GameObject.FindGameObjectsWithTag("WanderPoint");
        var waypoints = new Transform[_pointList.Length];
        var i = 0;
        foreach (var obj in _pointList)
        {
            waypoints[i] = obj.transform;
            i++;
        }
        
        // Retreat points list
        _retreatList = GameObject.FindGameObjectsWithTag("RetreatPoint");
        var retreatPoints = new Transform[_retreatList.Length];
        i = 0;
        foreach (var obj in _retreatList)
        {
            retreatPoints[i] = obj.transform;
            i++;
        }

        // Patrol
        var patrolState = new PatrolState(waypoints, playerNearRadius, patrollingRadius);
        patrolState.AddTransition(Transition.SawPlayer, FSMStateID.Chasing);
        patrolState.AddTransition(Transition.Damaged, FSMStateID.Retreating);
        patrolState.AddTransition(Transition.NoHealth, FSMStateID.Dead);

        // Chase
        var chaseState = new ChaseState();
        chaseState.AddTransition(Transition.LostPlayer, FSMStateID.Patrolling);
        chaseState.AddTransition(Transition.ReachPlayer, FSMStateID.Attacking);
        chaseState.AddTransition(Transition.Damaged, FSMStateID.Retreating);
        chaseState.AddTransition(Transition.NoHealth, FSMStateID.Dead);

        // Attack
        var attackState = new AttackState(waypoints);
        attackState.AddTransition(Transition.LostPlayer, FSMStateID.Patrolling);
        attackState.AddTransition(Transition.SawPlayer, FSMStateID.Chasing);
        attackState.AddTransition(Transition.Damaged, FSMStateID.Retreating);
        attackState.AddTransition(Transition.NoHealth, FSMStateID.Dead);

        // Retreat
        var retreatState = new RetreatState(retreatPoints, _playerTransform);
        retreatState.AddTransition(Transition.LostPlayer, FSMStateID.Patrolling);
        retreatState.AddTransition(Transition.NoHealth, FSMStateID.Dead);

        // Die
        var deadState = new DeadState();
        deadState.AddTransition(Transition.NoHealth, FSMStateID.Dead);
        
        AddFSMState(patrolState);
        AddFSMState(chaseState);
        AddFSMState(attackState);
        AddFSMState(retreatState);
        AddFSMState(deadState);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.gameObject.tag);
        if (!collision.gameObject.CompareTag("Bullet")) return;
        health -= 20;
        if (health > 0) return;
        Debug.Log("NPC: Dead state");
        SetTransition(Transition.NoHealth);
        Explode();
    }

    private void Explode()
    {
        var randomX = Random.Range(10.0f, 30.0f);
        var randomZ = Random.Range(10.0f, 30.0f);
        for (var i = 0; i < 3; i++)
        {
            _rigidbody.AddExplosionForce(10000.0f,transform.position - new Vector3(randomX, 10.0f, randomZ),40.0f,10.0f);
            _rigidbody.velocity = transform.TransformDirection(new Vector3(randomX, 20.0f, randomZ));
        }
        Destroy(gameObject, 1.5f);
    }

    public void ShootBullet()
    {
        if (!(_elapsedTime >= _shootRate)) return;
        bullet.tag = "NPCBullet";
        bullet.layer = LayerMask.NameToLayer("NPC");
        Instantiate(bullet, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        _elapsedTime = 0.0f;
    }
    
    public void CallFriend(Vector3 destination)
    {
        // Instantiate friend
        var friend = Instantiate(tankNPC, destination, Quaternion.identity);
        // Rotate friend
        var friendTargetRotation = Quaternion.FromToRotation(Vector3.forward, destination - friend.transform.position);
        friend.transform.rotation = Quaternion.Slerp(friend.transform.rotation, friendTargetRotation, Time.deltaTime * 50.0f);
        // Move friend
        friend.transform.Translate(Vector3.forward * (Time.deltaTime * 100.0f));
    }
}
