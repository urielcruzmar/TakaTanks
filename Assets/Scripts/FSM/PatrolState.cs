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

    public override void CheckTransitionRules(Transform player, Transform npc)
    {
        // Check distance to player to chase
        /*if (Vector3.Distance(npc.position, player.position) <= _playerNearRadius)
        {
            Debug.Log("Chasing");
            NPCTankController npcTankController = npc.GetComponent<NPCTankController>();
            if (npcTankController != null)
            {
                npc.GetComponent<NPCTankController>().SetTransaction(Transition.SawPlayer);
            }
            else
            {
                Debug.LogError("NPCTankController not found");
            }
        }*/
    }

    public override void RunState(Transform player, Transform npc)
    {
        // Search new point if reached
        if (Vector3.Distance(npc.position, _destinationPosition) <= _patrolRadius)
        {
            Debug.Log("Destiny reached. Searching next point");
            FindNextPoint();
        }
        
        // Rotate
        Quaternion targetRotation = Quaternion.FromToRotation(Vector3.forward, _destinationPosition - npc.position);
        npc.rotation = Quaternion.Slerp(npc.rotation, targetRotation, Time.deltaTime * _currentRotationSpeed);
        
        // Move forward
        npc.Translate(Vector3.forward * Time.deltaTime * _currentSpeed);
    }

    private void FindNextPoint()
    {
        int randomIndex = Random.Range(0, _waypoints.Length);
        Vector3 randomPosition = Vector3.zero;
        _destinationPosition = _waypoints[randomIndex].position + randomPosition;
    }
}
