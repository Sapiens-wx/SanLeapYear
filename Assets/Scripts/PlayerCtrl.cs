using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class PlayerCtrl : MonoBehaviour
{
    public Animator swordAnimator;
    public BoxCollider2D bc;
    public SpriteRenderer spr, playerStateSpr;
    public float gravity, maxFallSpd;
    public float keyDownBuffTime;
    [Header("Movement")]
    public float xspd;
    [Header("Jump")]
    public KeyCode jumpKey;
    public float jumpHeight;
    public float jumpInterval;
    public float coyoteTime;
    [Header("Lava Jump")]
    public float lavaJumpHeight;
    public float lavaJumpInterval;
    [Header("Ground Check")]
    public Vector2 leftBot;
    public Vector2 rightBot;
    [Header("Ceiling Check")]
    public Vector2 leftTop;
    public Vector2 rightTop;
    [Header("Hit")]
    public float invincibleTime;
    public float hitAnimDuration, counterAnimDuration;
    public float timeStopInterval;

    [HideInInspector] public Rigidbody2D rgb;
    [HideInInspector] public Animator animator;

    //inputs
    [HideInInspector] public int inputx;

    [HideInInspector] public static PlayerCtrl inst;
    [HideInInspector] public Vector2 v; //velocity
    [HideInInspector] public bool hittable;
    [HideInInspector] public bool onGround, prevOnGround;
    [HideInInspector] public float jumpKeyDown;
    [HideInInspector] public bool jumpKeyUp;
    [HideInInspector] public float yspd, lavaJumpYSpd;
    [HideInInspector] public int dir;
    [HideInInspector] public MaterialPropertyBlock matPB;
    [HideInInspector] public Sequence hitAnim, counterAnim, invincibleAnim;
    [HideInInspector] public Collider2D hitBy;
    //read input
    bool readInput;
    // player state
    [HideInInspector] [SerializeField] public PlayerState playerState;
    public bool ReadInput{
        get=>readInput;
        set{
            readInput=value;
            if(!readInput){
                inputx=0;
            }
        }
    }
    public event Action OnPlayerHit;
    public int Dir{
        get=>dir;
        set{
            dir=value;
            leftTop.x*=-1;
            rightTop.x*=-1;
            leftBot.x*=-1;
            rightBot.x*=-1;
            transform.localScale=new Vector3(dir,1,1);
            return;
        }
    }
    void OnDrawGizmosSelected(){
        Gizmos.color=Color.green;
        //ground check
        Gizmos.DrawLine((Vector2)transform.position+leftBot, (Vector2)transform.position+rightBot);
        //ceiling check
        Gizmos.DrawLine((Vector2)transform.position+leftTop, (Vector2)transform.position+rightTop);
        //jump height
        Gizmos.DrawLine(transform.position+new Vector3(-.2f,0,0),transform.position+new Vector3(.2f,0,0));
        Gizmos.DrawLine(transform.position,transform.position+new Vector3(0,jumpHeight,0));
        Gizmos.DrawLine(transform.position+new Vector3(-.2f,jumpHeight,0),transform.position+new Vector3(.2f,jumpHeight,0));
    }
    void Awake(){
        inst=this;
    }
    // Start is called before the first frame update
    void Start()
    {
        readInput=true;
        hittable=true;
        Dir=-1;
        jumpKeyDown=-100;

        rgb=GetComponent<Rigidbody2D>();
        animator=GetComponent<Animator>();
        yspd=jumpHeight/jumpInterval-0.5f*gravity*jumpInterval;
        lavaJumpYSpd=lavaJumpHeight/lavaJumpInterval-0.5f*gravity*lavaJumpInterval;

        //hit animation
        matPB=new MaterialPropertyBlock();
        spr.GetPropertyBlock(matPB);
        matPB.SetFloat("_whiteAmount", .5f);
        spr.SetPropertyBlock(matPB);

        //invincible anim
        invincibleAnim=DOTween.Sequence();
        invincibleAnim.SetAutoKill(false);
        invincibleAnim.AppendCallback(()=>{
            hittable=false;
        });
        invincibleAnim.Append(DOTween.To(()=>matPB.GetFloat("_whiteAmount"), (val)=>{
            spr.GetPropertyBlock(matPB);
            matPB.SetFloat("_whiteAmount",val);
            spr.SetPropertyBlock(matPB);
        }, 0, hitAnimDuration).SetLoops(Mathf.RoundToInt(invincibleTime/hitAnimDuration), LoopType.Yoyo).SetEase(Ease.InOutQuad));
        invincibleAnim.AppendCallback(()=>{
            hittable=true;
            spr.GetPropertyBlock(matPB);
            matPB.SetFloat("_whiteAmount",.5f);
            spr.SetPropertyBlock(matPB);
        });
        invincibleAnim.Pause();

        hitAnim=DOTween.Sequence();
        hitAnim.SetAutoKill(false);
        hitAnim.AppendCallback(()=>{
            spr.GetPropertyBlock(matPB);
            matPB.SetFloat("_whiteAmount",0);
            spr.SetPropertyBlock(matPB);
            hittable=false;
        });
        //hitAnim.AppendCallback(()=>StartCoroutine(PauseForSeconds(.5f)));
        hitAnim.AppendInterval(0.01f);
        hitAnim.Append(DOTween.To(()=>matPB.GetFloat("_whiteAmount"), (val)=>{
            spr.GetPropertyBlock(matPB);
            matPB.SetFloat("_whiteAmount",val);
            spr.SetPropertyBlock(matPB);
        }, .5f, counterAnimDuration));
        hitAnim.AppendCallback(()=>invincibleAnim.Restart());
        hitAnim.Pause();

        //counter animation
        counterAnim=DOTween.Sequence();
        counterAnim.SetAutoKill(false);
        counterAnim.AppendCallback(()=>{
            spr.GetPropertyBlock(matPB);
            matPB.SetFloat("_whiteAmount",1);
            spr.SetPropertyBlock(matPB);
            //Time.timeScale=.1f;
        });
        counterAnim.AppendCallback(()=>StartCoroutine(PauseForSeconds(timeStopInterval)));
        counterAnim.AppendInterval(.01f);
        //counterAnim.AppendCallback(()=>Time.timeScale=1);
        counterAnim.Append(DOTween.To(()=>matPB.GetFloat("_whiteAmount"), (val)=>{
            spr.GetPropertyBlock(matPB);
            matPB.SetFloat("_whiteAmount",val);
            spr.SetPropertyBlock(matPB);
        }, .5f, counterAnimDuration));
        counterAnim.Pause();

        SwitchState(PlayerState.Normal);
    }

    // Update is called once per frame
    void Update()
    {
        if(readInput){
            if(Input.GetKeyDown(jumpKey))
                jumpKeyDown=Time.time;
            else if(Input.GetKeyUp(jumpKey))
                jumpKeyUp=true;
        }
    }
    void FixedUpdate(){
        HandleInputs();
        CheckOnGround();
        /*
        Movement();
        Jump();
        CeilingCheck();
        ApplyGravity();
        */
        UpdateVelocity();
    }
    void HandleInputs(){
        if(readInput)
            inputx=(int)Input.GetAxisRaw("Horizontal");
    }
    void UpdateVelocity(){
        rgb.velocity=v;
    }
    void CheckOnGround(){
        prevOnGround=onGround;
        Collider2D hit = Physics2D.OverlapArea((Vector2)transform.position+leftBot, (Vector2)transform.position+rightBot, GameManager.inst.groundLayer);
        onGround=hit;
    }
    IEnumerator PauseForSeconds(float sec){
        Time.timeScale=0;
        yield return new WaitForSecondsRealtime(sec);
        Time.timeScale=1;
    }
    public void SwitchState(PlayerState state)
    {
        playerState=state;
        switch (state)
        {
            case PlayerState.Normal:
                GameManager.inst.waterCollider.isTrigger=false;
                playerStateSpr.sprite=GameManager.inst.normalSpr;
                break;
            case PlayerState.LavaDeath:
                GameManager.inst.waterCollider.isTrigger=true;
                playerStateSpr.sprite=GameManager.inst.lavaSpr;
                break;
            case PlayerState.WaterDeath:
                GameManager.inst.waterCollider.isTrigger=false;
                playerStateSpr.sprite=GameManager.inst.waterSpr;
                break;
        }
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        bool isLava=GameManager.IsLayer(GameManager.inst.lavaLayer, collision.gameObject.layer);
        bool isWater=GameManager.IsLayer(GameManager.inst.waterLayer, collision.gameObject.layer);
        bool isNormal=GameManager.IsLayer(GameManager.inst.normalLayer, collision.gameObject.layer);
        switch (playerState)
        {
            case PlayerState.Normal:
                if (isLava || isWater) {
                    SwitchState(isLava ? PlayerState.LavaDeath : PlayerState.WaterDeath);
                    RevivePoint.Revive();
                }
                break;
            case PlayerState.LavaDeath:
                if (isNormal) {
                    SwitchState(PlayerState.Normal);
                    RevivePoint.Revive();
                }
                break;
            case PlayerState.WaterDeath:
                if (isNormal) {
                    SwitchState(PlayerState.Normal);
                    RevivePoint.Revive();
                } else if (isLava) {
                    v.y=lavaJumpYSpd;
                }
                break;
        }
    }
    //0b1000 is the mask for slash
    public enum AttackType{
        SlashHorizontal=0b1001,
        SlashUp=0b1010,
        SlashDown=0b1011,
        Counter=0,
        Throw=1,
        Other=2,
        Mask_Slash=0b1000
    }
}

public enum PlayerState
{
    Normal,
    LavaDeath,
    WaterDeath
}