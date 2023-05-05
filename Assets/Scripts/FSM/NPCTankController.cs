using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class NPCTankController : AdvanceFSM
{
    [SerializeField] private GameObject tankNPC;
    
    public GameObject Bullet;
    public int Health;

    private Rigidbody _rigidbody;
    protected Transform playerTransform;
    // Points
    protected Vector3 nextPosition;
    protected GameObject[] pointList;
    protected GameObject[] retreatList;
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
        Health = 100;
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
        CurrentState.CheckTransitionRules(playerTransform, gameObject);
        CurrentState.RunState(playerTransform, gameObject);
    }

    public void SetTransition(Transition transition)
    {
        PerformTransition(transition);
    }

    private void ConstructFSM()
    {
        // Waypoints list
        pointList = GameObject.FindGameObjectsWithTag("WanderPoint");
        Transform[] waypoints = new Transform[pointList.Length];
        int i = 0;
        foreach (GameObject obj in pointList)
        {
            waypoints[i] = obj.transform;
            i++;
        }
        
        // Retreat points list
        retreatList = GameObject.FindGameObjectsWithTag("RetreatPoint");
        Transform[] retreatPoints = new Transform[retreatList.Length];
        i = 0;
        foreach (GameObject obj in retreatList)
        {
            retreatPoints[i] = obj.transform;
            i++;
        }

        // Patrol
        PatrolState patrolState = new PatrolState(waypoints, playerNearRadius, patrollingRadius);
        patrolState.AddTransition(Transition.SawPlayer, FSMStateID.Chasing);
        patrolState.AddTransition(Transition.Damaged, FSMStateID.Retreating);
        patrolState.AddTransition(Transition.NoHealth, FSMStateID.Dead);

        // Chase
        ChaseState chaseState = new ChaseState();
        chaseState.AddTransition(Transition.LostPlayer, FSMStateID.Patrolling);
        chaseState.AddTransition(Transition.ReachPlayer, FSMStateID.Attacking);
        chaseState.AddTransition(Transition.Damaged, FSMStateID.Retreating);
        chaseState.AddTransition(Transition.NoHealth, FSMStateID.Dead);

        // Attack
        AttackState attackState = new AttackState(waypoints);
        attackState.AddTransition(Transition.LostPlayer, FSMStateID.Patrolling);
        attackState.AddTransition(Transition.SawPlayer, FSMStateID.Chasing);
        attackState.AddTransition(Transition.Damaged, FSMStateID.Retreating);
        attackState.AddTransition(Transition.NoHealth, FSMStateID.Dead);

        // Retreat
        RetreatState retreatState = new RetreatState(retreatPoints, playerTransform);
        //retreatState.AddTransition(Transition.ReachPlayer, FSMStateID.Chasing);
        retreatState.AddTransition(Transition.LostPlayer, FSMStateID.Patrolling);
        retreatState.AddTransition(Transition.NoHealth, FSMStateID.Dead);

        // Die
        DeadState deadState = new DeadState();
        deadState.AddTransition(Transition.NoHealth, FSMStateID.Dead);
        
        AddFSMState(patrolState);
        AddFSMState(chaseState);
        AddFSMState(attackState);
        AddFSMState(retreatState);
        AddFSMState(deadState);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            Health -= 20;
            if (Health <= 0)
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
            Bullet.tag = "NPCBullet";
            Instantiate(Bullet, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
            elapsedTime = 0.0f;
        }
    }
    
    public void CallFriend(Vector3 destination)
    {
        // Instantiate friend
        GameObject friend = Instantiate(tankNPC, destination, Quaternion.identity);
        // Rotate friend
        Quaternion friendTargetRotation = Quaternion.FromToRotation(Vector3.forward, destination - friend.transform.position);
        friend.transform.rotation = Quaternion.Slerp(friend.transform.rotation, friendTargetRotation, Time.deltaTime * 50.0f);
        // Move friend
        friend.transform.Translate(Vector3.forward * (Time.deltaTime * 100.0f));
    }
}
