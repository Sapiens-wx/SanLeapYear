using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PStateBase : StateMachineBehaviour
{
    internal PlayerCtrl player;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(player==null) player=PlayerCtrl.inst;
    }
    //execution order 
    /*
    void FixedUpdate(){
        CheckWall();
        Movement();
        Jump();
        CheckOnGround();
        CeilingCheck();
        ApplyGravity();
        UpdateVelocity();
    }*/
    internal void UpdateVelocity(){
        player.rgb.velocity=player.v;
    }
    virtual internal void ApplyGravity(){
        if(player.onGround){
            if(!player.prevOnGround && player.v.y<0) //on ground enter
                player.v.y=0;
        }//if player is not wall jumping, is on wall, and is pressing the button the opposite dir of [dir], then the player should cling on the wall
        else if(player.v.y>=player.maxFallSpd)
            player.v.y+=player.gravity*Time.fixedDeltaTime;
    }
    virtual internal void Movement(){
        player.v.x=player.xspd*player.inputx;
        //change direction
        if(player.inputx!=0 && player.inputx!=-player.dir){
            player.Dir=-player.inputx;
        }
    }
    internal virtual void CeilingCheck(){
        LayerMask ceilingLayer=GameManager.inst.groundLayer;
        //if the player is bounced up by lava, keep the momentum
        if(PlayerCtrl.inst.playerState==PlayerState.WaterDeath)
            ceilingLayer^=GameManager.inst.normalLayer;
        if(Physics2D.OverlapArea((Vector2)player.transform.position+player.leftTop, (Vector2)player.transform.position+player.rightTop, ceilingLayer)){
            if(player.v.y>0){
                player.v.y=0;
                player.animator.SetTrigger("jump_down");
            } 
        }
    }
    virtual internal void Jump(){
        throw new System.Exception("Jump function not implemented");
    }
    internal IEnumerator InvincibleTimer(){
        player.hitAnim.Restart();
        yield return new WaitForSeconds(player.invincibleTime);
        player.hittable=true;
        player.hitAnim.Pause();
        player.spr.GetPropertyBlock(player.matPB);
        player.matPB.SetFloat("_whiteAmount",.5f);
        player.spr.SetPropertyBlock(player.matPB);
    }
}
