using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : FSMState
{
    private Vector3 _destinationPosition;
    private Transform[] _waypoints;
    private float _currentRotationSpeed = 1.0f;
    private float _currentSpeed = 100.0f;

    public AttackState(Transform[] wp)
    {
        _waypoints = wp;
        StateID = FSMStateID.Attacking;
        _currentRotationSpeed = 1.0f;
        _currentSpeed = 100.0f;
        FindNextPoint();
    }

    public override void CheckTransitionRules(Transform player, Transform npc)
    {
        float distance = Vector3.Distance(npc.position, player.position);
        if (distance is >= 200.0f and < 300.0f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(_destinationPosition - npc.position);
            npc.rotation = Quaternion.Slerp(npc.rotation, targetRotation, Time.deltaTime * _currentRotationSpeed);
            npc.Translate(Vector3.forward * (Time.deltaTime * _currentSpeed));
            Debug.Log("NPC: Chasing");
            npc.GetComponent<NPCTankController>().SetTransition(Transition.SawPlayer);
        }
        else if (distance >= 300.0f)
        {
            Debug.Log("NPC: Patrolling");
            npc.GetComponent<NPCTankController>().SetTransition(Transition.LostPlayer);
        }
    }

    public override void RunState(Transform player, Transform npc)
    {
        _destinationPosition = player.position;
        Quaternion targetRotation = Quaternion.LookRotation(_destinationPosition - npc.position);
        npc.rotation = Quaternion.Slerp(npc.rotation, targetRotation, Time.deltaTime * _currentRotationSpeed);
        npc.GetComponent<NPCTankController>().ShootBullet();
    }
    
    private void FindNextPoint()
    {
        int randomIndex = Random.Range(0, _waypoints.Length);
        Vector3 randomPosition = Vector3.zero;
        _destinationPosition = _waypoints[randomIndex].position + randomPosition;
    }
}
