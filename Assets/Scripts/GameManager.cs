using UnityEngine;

public class GameManager : MonoBehaviour{
    public LayerMask groundLayer,playerLayer;
    public LayerMask lavaLayer, waterLayer, glassLayer, normalLayer;
    public CompositeCollider2D lavaCollider, waterCollider, normalCollider, glassCollider;
    public Sprite lavaSpr, waterSpr, normalSpr;

    public static GameManager inst;
    void Awake(){
        inst=this;
    }
    public static bool IsLayer(LayerMask mask, int layer){
        return ((1<<layer)&mask.value)!=0;
    }
}