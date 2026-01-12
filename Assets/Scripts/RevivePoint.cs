using UnityEngine;

public class RevivePoint : MonoBehaviour
{
    public static RevivePoint lastRevivePoint;
    public bool defaultRevivePoint;
    void Awake() {
        if (defaultRevivePoint)
        {
            lastRevivePoint=this;
        }
    }
    public static void Revive() {
        Vector2 pos=lastRevivePoint.transform.position;
        PlayerCtrl.inst.transform.position=pos;
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if(GameManager.IsLayer(GameManager.inst.playerLayer, collision.gameObject.layer))
        {
            lastRevivePoint=this;
        }
    }
}