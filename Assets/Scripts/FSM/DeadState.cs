using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadState : FSMState
{
    public DeadState()
    {
        StateID = FSMStateID.Dead;
    }
    
    public override void CheckTransitionRules(Transform player, Transform npc)
    {
        
    }

    public override void RunState(Transform player, Transform npc)
    {
        
    }
}
