using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolState : FSMState
{
    private Vector3 _destinationPosition;
    private Transform[] _waypoints;
    private float _currentRotationSpeed = 1.0f;
    private float _currentSpeed = 100.0f;
    private float _playerNearRadius;
    private float _patrolRadius;

    public PatrolState(Transform[] wp, float playerNearRadius, float patrolRadius)
    {
        _waypoints = wp;
        StateID = FSMStateID.Patrolling;
        _playerNearRadius = playerNearRadius;
        _patrolRadius = patrolRadius;
    }

    public override void CheckTransitionRules(Transform player, GameObject npc)
    {
        // Check health
        var controller = npc.GetComponent<NPCTankController>();
        if (controller.health < 50)
        {
            controller.SetTransition(Transition.Damaged);
            return;
        }
        // Check distance to player to chase
        if (Vector3.Distance(npc.transform.position, player.position) <= _playerNearRadius)
        {
            if (controller != null)
            {
                controller.SetTransition(Transition.SawPlayer);
            }
            else
            {
                Debug.LogError("NPCTankController not found");
            }
        }
    }

    public override void RunState(Transform player, GameObject npc)
    {
        // Search new point if reached
        if (Vector3.Distance(npc.transform.position, _destinationPosition) <= _patrolRadius)
        {
            FindNextPoint();
        }
        
        // Rotate
        Quaternion targetRotation = Quaternion.FromToRotation(Vector3.forward, _destinationPosition - npc.transform.position);
        npc.transform.rotation = Quaternion.Slerp(npc.transform.rotation, targetRotation, Time.deltaTime * _currentRotationSpeed);
        
        // Move forward
        npc.transform.Translate(Vector3.forward * (Time.deltaTime * _currentSpeed));
    }

    private void FindNextPoint()
    {
        int randomIndex = Random.Range(0, _waypoints.Length);
        Vector3 randomPosition = Vector3.zero;
        _destinationPosition = _waypoints[randomIndex].position + randomPosition;
    }
}
