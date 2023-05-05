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

    public override void CheckTransitionRules(Transform player, GameObject npc)
    {
        // Check health
        var controller = npc.GetComponent<NPCTankController>();
        if (controller.Health < 50)
        {
            controller.SetTransition(Transition.Damaged);
            return;
        }
        // Check distance
        float distance = Vector3.Distance(npc.transform.position, player.position);
        if (distance is >= 200.0f and < 300.0f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(_destinationPosition - npc.transform.position);
            npc.transform.rotation = Quaternion.Slerp(npc.transform.rotation, targetRotation, Time.deltaTime * _currentRotationSpeed);
            npc.transform.Translate(Vector3.forward * (Time.deltaTime * _currentSpeed));
            Debug.Log("NPC: Chasing");
            controller.SetTransition(Transition.SawPlayer);
        }
        else if (distance >= 300.0f)
        {
            Debug.Log("NPC: Patrolling");
            npc.GetComponent<NPCTankController>().SetTransition(Transition.LostPlayer);
        }
    }

    public override void RunState(Transform player, GameObject npc)
    {
        _destinationPosition = player.position;
        Quaternion targetRotation = Quaternion.LookRotation(_destinationPosition - npc.transform.position);
        npc.transform.rotation = Quaternion.Slerp(npc.transform.rotation, targetRotation, Time.deltaTime * _currentRotationSpeed);
        npc.GetComponent<NPCTankController>().ShootBullet();
    }
    
    private void FindNextPoint()
    {
        int randomIndex = Random.Range(0, _waypoints.Length);
        Vector3 randomPosition = Vector3.zero;
        _destinationPosition = _waypoints[randomIndex].position + randomPosition;
    }
}
