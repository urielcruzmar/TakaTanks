using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class NPCTankController : AdvanceFSM
{
    public GameObject Bullet;
    private int _health;
    private Rigidbody _rigidbody;
    protected Transform playerTransform;
    // Points
    protected Vector3 nextPosition;
    protected GameObject[] pointList;
    // Shooting
    protected float shootRate;
    protected float elapsedTime;
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
        _health = 100;
        elapsedTime = 0.0f;
        shootRate = 2.0f;
        // Get enemy
        GameObject objPlayer = GameObject.FindGameObjectWithTag("Player");
        playerTransform = objPlayer.transform;
        // Rigidbody
        _rigidbody = GetComponent<Rigidbody>();
        if (!playerTransform)
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
        elapsedTime += Time.deltaTime;
    }

    protected override void FSMFixedUpdate()
    {
        CurrentState.CheckTransitionRules(playerTransform, transform);
        CurrentState.RunState(playerTransform, transform);
    }

    public void SetTransition(Transition transition)
    {
        PerformTransition(transition);
    }

    private void ConstructFSM()
    {
        pointList = GameObject.FindGameObjectsWithTag("WanderPoint");
        
        Transform[] waypoints = new Transform[pointList.Length];
        int i = 0;
        foreach (GameObject obj in pointList)
        {
            waypoints[i] = obj.transform;
            i++;
        }

        PatrolState patrolState = new PatrolState(waypoints, playerNearRadius, patrollingRadius);
        patrolState.AddTransition(Transition.SawPlayer, FSMStateID.Chasing);
        patrolState.AddTransition(Transition.NoHealth, FSMStateID.Dead);

        ChaseState chaseState = new ChaseState(waypoints);
        chaseState.AddTransition(Transition.LostPlayer, FSMStateID.Patrolling);
        chaseState.AddTransition(Transition.ReachPlayer, FSMStateID.Attacking);
        chaseState.AddTransition(Transition.NoHealth, FSMStateID.Dead);

        AttackState attackState = new AttackState(waypoints);
        attackState.AddTransition(Transition.LostPlayer, FSMStateID.Patrolling);
        attackState.AddTransition(Transition.SawPlayer, FSMStateID.Chasing);
        attackState.AddTransition(Transition.NoHealth, FSMStateID.Dead);

        DeadState deadState = new DeadState();
        deadState.AddTransition(Transition.NoHealth, FSMStateID.Dead);
        
        AddFSMState(patrolState);
        AddFSMState(chaseState);
        AddFSMState(attackState);
        AddFSMState(deadState);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            _health -= 50;
            if (_health <= 0)
            {
                Debug.Log("NPC: Dead state");
                SetTransition(Transition.NoHealth);
                Explode();
            }
        }
    }

    private void Explode()
    {
        float randomX = Random.Range(10.0f, 30.0f);
        float randomZ = Random.Range(10.0f, 30.0f);
        for (int i = 0; i < 3; i++)
        {
            _rigidbody.AddExplosionForce(10000.0f,transform.position - new Vector3(randomX, 10.0f, randomZ),40.0f,10.0f);
            _rigidbody.velocity = transform.TransformDirection(new Vector3(randomX, 20.0f, randomZ));
        }
        Destroy(gameObject, 1.5f);
    }

    public void ShootBullet()
    {
        if (elapsedTime >= shootRate)
        {
            Instantiate(Bullet, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
            elapsedTime = 0.0f;
        }
    }
}
