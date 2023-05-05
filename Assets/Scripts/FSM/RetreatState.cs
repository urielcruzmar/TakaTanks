using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RetreatState : FSMState
{
    private Transform[] _retreatPoints;
    private Vector3 _destinationPosition = Vector3.zero;
    private float _currentRotationSpeed = 50.0f;
    private float _currentSpeed = 100.0f;
    private bool _retreatCompleted = false;

    public RetreatState(Transform[] retreatPoints, Transform playerTransform)
    {
        StateID = FSMStateID.Retreating;
        _retreatPoints = retreatPoints;
    }

    public override void CheckTransitionRules(Transform player, GameObject npc)
    {
        // If not retreated, do not transition
        if (!_retreatCompleted) return;
        // Transition to search player in las location
        var controller = npc.GetComponent<NPCTankController>();
        float distance = Vector3.Distance(npc.transform.position, _destinationPosition);
        if (distance <= 300.0f)
        {
            _destinationPosition = Vector3.zero;
            controller.SetTransition(Transition.LostPlayer);
        }
    }

    public override void RunState(Transform player, GameObject npc)
    {
        // Check if retreat point set
        if (_destinationPosition.Equals(Vector3.zero))
        {
            FindRetreatPoint();
        }
        // Check distance to retreat point
        else if (Vector3.Distance(npc.transform.position, _destinationPosition) <= 100.0f)
        {
            // Restore health
            npc.GetComponent<NPCTankController>().health = 100;
            // Call reinforcements
            npc.GetComponent<NPCTankController>().CallFriend(_destinationPosition);
            // Change destination to player position
            _destinationPosition = player.position;
            _retreatCompleted = true;
        }
        // Move to position
        // Rotate npc
        var position = npc.transform.position;
        var lookPos = _destinationPosition - position;
        Quaternion targetRotation = Quaternion.FromToRotation(Vector3.forward, lookPos);
        npc.transform.rotation = Quaternion.Slerp(npc.transform.rotation, targetRotation, Time.deltaTime * _currentRotationSpeed);
        // Move npc
        npc.transform.Translate(Vector3.forward * (Time.deltaTime * _currentSpeed));
        
    }

    private void FindRetreatPoint()
    {
        int randomIndex = Random.Range(0, _retreatPoints.Length);
        Vector3 randomPosition = Vector3.zero;
        _destinationPosition = _retreatPoints[randomIndex].position + randomPosition;
    }
}
