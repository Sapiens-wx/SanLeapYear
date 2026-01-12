using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class Pset_params : PStateBase
{
    public bool setHittable,hittableValue;
    private bool hitOriginalValue;
    //Coroutine coro;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        player.v.x=0;
        if(setHittable){
            hitOriginalValue=player.hittable;
            player.hittable = hittableValue;
        }
        //coro = player.StartCoroutine(m_FixedUpdate());
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(setHittable){
            player.hittable = hitOriginalValue;
        }
        //player.StopCoroutine(coro);
        //coro=null;
    }
}
