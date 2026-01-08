using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI設定")]
    [SerializeField] private GameObject gameClearUI;

    [Header("花火のエリア設定（ドーナツ型）")]
    [SerializeField] private GameObject fireworkPrefab;
    [SerializeField] private Transform fireworkCenter;  // 中心点（プレイヤーなどを指定）
    
    [Tooltip("中心からどれくらい離れるか（内側の半径）")]
    [SerializeField] private float minRadius = 5.0f; 
    
    [Tooltip("最大でどれくらい離れるか（外側の半径）")]
    [SerializeField] private float maxRadius = 15.0f; 

    [Tooltip("上下のばらつき")]
    [SerializeField] private float heightRange = 5.0f;

    [Header("花火のタイミング設定")]
    [SerializeField] private float minInterval = 0.2f; // 連発感を出すために少し短くしました
    [SerializeField] private float maxInterval = 1.0f;

    private int enemyCount = 0;
    private bool isGameClear = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        AudioManager.Instance.PlayBGM("BGM");
        if (gameClearUI != null) gameClearUI.SetActive(false);
    }

    public void RegisterEnemy()
    {
        enemyCount++;
        Debug.Log($"敵追加: 残り {enemyCount}体");
    }

    public void ReportEnemyDeath()
    {
        enemyCount--;
        Debug.Log($"敵撃破: 残り {enemyCount}体");

        if (enemyCount <= 0 && !isGameClear)
        {
            GameClear();
        }
    }

    private void GameClear()
    {
        isGameClear = true;
        Debug.Log("Game Clear!!");

        if (gameClearUI != null) gameClearUI.SetActive(true);

        AudioManager.Instance.PlaySE("Win"); 
        AudioManager.Instance.StopBGM();
        AudioManager.Instance.PlayBGM("LastBGM"); 

        if (fireworkPrefab != null)
        {
            StartCoroutine(LaunchFireworksLoop());
        }
    }

    // ★変更: ドーナツ状の範囲でランダムに生成するコルーチン
    private IEnumerator LaunchFireworksLoop()
    {
        while (true)
        {
            Vector3 centerPos = (fireworkCenter != null) ? fireworkCenter.position : transform.position;

            // 1. ランダムな角度を決める (0 ～ 360度)
            float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad; // ラジアンに変換

            // 2. ランダムな距離を決める (内側の半径 ～ 外側の半径)
            float randomRadius = Random.Range(minRadius, maxRadius);

            // 3. 角度と距離から X, Z 座標を計算 (三角関数)
            float posX = Mathf.Cos(randomAngle) * randomRadius;
            float posZ = Mathf.Sin(randomAngle) * randomRadius;

            // 4. 高さはランダム
            float posY = Random.Range(-heightRange / 2f, heightRange / 2f);

            // 5. 最終的な座標決定
            Vector3 spawnPos = centerPos + new Vector3(posX, posY, posZ);
            Quaternion lookUp = Quaternion.Euler(-90, 0, 0);

            Instantiate(fireworkPrefab, spawnPos, lookUp);

            float waitTime = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(waitTime);
        }
    }

    // ★変更: エディタ上でドーナツ型の範囲を見やすく表示
    private void OnDrawGizmosSelected()
    {
        Vector3 centerPos = (fireworkCenter != null) ? fireworkCenter.position : transform.position;
        Gizmos.color = Color.cyan;

        // 内側の円と外側の円を簡易的に描画
        DrawCircle(centerPos, minRadius);
        DrawCircle(centerPos, maxRadius);

        // 高さがわかるように上下の線も引く
        Gizmos.DrawLine(centerPos + Vector3.up * heightRange / 2f, centerPos - Vector3.up * heightRange / 2f);
    }

    // 円を描く補助関数
    private void DrawCircle(Vector3 center, float radius)
    {
        float step = 10.0f; // 刻み幅
        for (float angle = 0; angle < 360; angle += step)
        {
            Vector3 p1 = center + new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad) * radius, 0, Mathf.Sin(angle * Mathf.Deg2Rad) * radius);
            Vector3 p2 = center + new Vector3(Mathf.Cos((angle + step) * Mathf.Deg2Rad) * radius, 0, Mathf.Sin((angle + step) * Mathf.Deg2Rad) * radius);
            Gizmos.DrawLine(p1, p2);
        }
    }
}