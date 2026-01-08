using UnityEngine;

public class FireworkSound : MonoBehaviour
{

    void Start()
    {
        // 1. 最初にひゅるひゅる
        AudioManager.Instance.PlaySE("whistleSE", transform.position, 1.0f, 0.8f, 10.0f, 200.0f);
        
        // 2. 1.5秒後（パーティクルのDurationと同じ時間）に爆発音を予約
        Invoke("PlayExplosion", 1.5f); 
    }

    void PlayExplosion()
    {
        AudioManager.Instance.PlaySE("explosionSE", transform.position, 1.0f, 0.8f, 10.0f, 200.0f);
    }
}