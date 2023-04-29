using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum Transition
{
        None = 0,
        SawPlayer,
        ReachPlayer,
        LostPlayer,
        NoHealth
}

public enum FSMStateID
{
    None = 0,
    Patrolling,
    Chasing,
    Attacking,
    Dead
}

public class AdvanceFSM : FSM
{
    private List<FSMState> _fsmStates;

    private FSMStateID _currentStateID;
    public FSMStateID CurrentStateID => _currentStateID;

    private FSMState _currentState;
    public FSMState CurrentState => _currentState;

    public AdvanceFSM()
    {
        _fsmStates = new List<FSMState>();
    }
    
    // Add new state
    public void AddFSMState(FSMState fsmState)
    {
        // First state = initial state
        if (_fsmStates.Count == 0)
        {
            _fsmStates.Add(fsmState);
            _currentState = fsmState;
            _currentStateID = fsmState.ID;
            return;
        }

        foreach (FSMState state in _fsmStates)
        {
            if (state.ID == fsmState.ID)
            {
                Debug.LogError("FSM ERROR: Adding existing state");
                return;
            }
            _fsmStates.Add(fsmState);
        }
    }
    
    // Delete state
    public void DeleteState(FSMStateID fsmStateID)
    {
        if (fsmStateID == FSMStateID.None)
        {
            Debug.LogError("FSM ERROR: Null state not allowed");
            return;
        }

        foreach (var state in _fsmStates.Where(state => state.ID == fsmStateID))
        {
            _fsmStates.Remove(state);
            return;
        }
        
        Debug.LogError("FSM ERROR: State does not exist, cannot remove");
    }
    
    // Transition
    public void PerformTransition(Transition transition)
    {
        if (transition == Transition.None)
        {
            Debug.LogError("FSM ERROR: Null transition not allowed");
            return;
        }

        var id = _currentState.GetOutputState(transition);
        if (id == FSMStateID.None)
        {
            Debug.LogError("FSM ERROR: Transition does not exist");
            return;
        }

        _currentStateID = id;
        foreach (var state in _fsmStates.Where(state => state.ID == _currentStateID))
        {
            _currentState = state;
            break;
        }
    }
}
