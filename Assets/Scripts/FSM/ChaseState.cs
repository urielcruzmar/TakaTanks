using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseState : FSMState
{
    private Vector3 _destinationPosition;
    private float _currentRotationSpeed;
    private float _currentSpeed = 100.0f;

    public ChaseState(Transform[] wp)
    {
        StateID = FSMStateID.Chasing;
    }
    
    public override void CheckTransitionRules(Transform player, Transform npc)
    {
        _destinationPosition = player.position;
        float distance = Vector3.Distance(npc.position, _destinationPosition);
        if (distance <= 200.0f)
        {
            Debug.Log("NPC: Attacking");
            npc.GetComponent<NPCTankController>().SetTransition(Transition.ReachPlayer);
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
        npc.Translate(Vector3.forward * (Time.deltaTime * _currentSpeed));
    }
}
