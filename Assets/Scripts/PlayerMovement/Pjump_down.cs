using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class Pjump_down : PStateBase
{
    Coroutine coro;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        coro = player.StartCoroutine(m_FixedUpdate());
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        player.StopCoroutine(coro);
        coro=null;
    }
    IEnumerator m_FixedUpdate(){
        WaitForFixedUpdate wait=new WaitForFixedUpdate();
        while(true){
            Movement();
            ApplyGravity();
            CeilingCheck();
            yield return wait;
        }
    }
    internal override void CeilingCheck(){
        LayerMask ceilingLayer=GameManager.inst.groundLayer;
        //if the player is bounced up by lava, keep the momentum
        if(PlayerCtrl.inst.playerState==PlayerState.WaterDeath)
            ceilingLayer^=GameManager.inst.normalLayer;
        if(Physics2D.OverlapArea((Vector2)player.transform.position+player.leftTop, (Vector2)player.transform.position+player.rightTop, ceilingLayer)){
            if(player.v.y>0){
                player.v.y=0;
            } 
        }
    }
    override internal void ApplyGravity(){
        if(player.onGround && player.v.y<0){
            player.v.y=0;
            player.animator.SetTrigger("idle");
        }
        else if(player.v.y>=player.maxFallSpd)
            player.v.y+=player.gravity*Time.fixedDeltaTime;
    }
}
