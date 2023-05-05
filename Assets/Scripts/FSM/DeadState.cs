using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadState : FSMState
{
    public DeadState()
    {
        StateID = FSMStateID.Dead;
    }
    
    public override void CheckTransitionRules(Transform player, GameObject npc)
    {
        
    }

    public override void RunState(Transform player, GameObject npc)
    {
        
    }
}
