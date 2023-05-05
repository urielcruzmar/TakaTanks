using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class FSMState
{
    protected Dictionary<Transition, FSMStateID> _map = new Dictionary<Transition, FSMStateID>();
    protected FSMStateID StateID;
    public FSMStateID ID => StateID;

    // Add transition to map
    public void AddTransition(Transition transition, FSMStateID id)
    {
        // Check null
        if (transition == Transition.None || id == FSMStateID.None)
        {
            Debug.LogError("FSMState ERROR: Transition or null state not allowed");
            return;
        }
        
        // Check if transition is in map
        if (_map.ContainsKey(transition))
        {
            Debug.LogError("FSMState ERROR: Transition already in collection");
            return;
        }
        
        // Add transition
        _map.Add(transition, id);
        Debug.Log("Transition " + transition + " added");
    }
    
    // Delete transition
    public void DeleteTransition(Transition transition)
    {
        // Check null
        if (transition == Transition.None)
        {
            Debug.LogError("FSMState ERROR: Null transition not allowed");
            return;
        }
        
        // Check if transition is in map and delete
        if (_map.ContainsKey(transition))
        {
            _map.Remove(transition);
        }
        
        // Transition not found
        Debug.LogError("FSMState ERROR: Transition not in collection");
    }
    
    // Get state when transition
    public FSMStateID GetOutputState(Transition transition)
    {
        // Check null
        if (transition == Transition.None)
        {
            Debug.LogError("FSMState ERROR: Null transition not allowed");
            return FSMStateID.None;
        }
        
        // Check if transition is in map
        if (_map.ContainsKey(transition))
        {
            return _map[transition];
        }
        
        // Transition not found
        Debug.LogError("FSMState ERROR: Transition not in collection");
        return FSMStateID.None;
    }
    
    // Check if state has to make transition
    public abstract void CheckTransitionRules(Transform player, GameObject npc);
    
    // NPC Control
    public abstract void RunState(Transform player, GameObject npc);

    protected bool IsInCurrentRange(Transform transform, Vector3 position)
    {
        var transformPosition = transform.position;
        var xPosition = Mathf.Abs(position.x - transformPosition.x);
        var zPosition = Mathf.Abs(position.z - transformPosition.z);
        return xPosition <= 50 && zPosition <= 50;
    }
}
