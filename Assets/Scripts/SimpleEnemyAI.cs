using UnityEngine;
using UnityEngine.AI; // NavMeshを使うために必要

// 自動で必要なコンポーネントを追加
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class SimpleEnemyAI : MonoBehaviour
{
    [Header("基本設定")]
    [SerializeField] private string targetTag = "Player"; // 追いかける相手のタグ
    [SerializeField] private float maxHP = 100f;          // 体力
    [SerializeField] private float stopDistance = 1.5f;   // 攻撃のために止まる距離
    [SerializeField] private float detectionRange = 10.0f; // プレイヤーを検知する範囲
    private NavMeshAgent agent;
    private Animator animator;
    private Transform target;
    private float currentHP;
    private bool isDead = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        currentHP = maxHP;

        // 追いかける対象（VRプレイヤー）を探す
        GameObject playerObj = GameObject.FindGameObjectWithTag(targetTag);
        if (playerObj != null)
        {
            target = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("ターゲットが見つかりません！VRカメラに 'Player' タグをつけてください。");
        }
    }

    void Update()
    {
        if (isDead || target == null) return;

        // プレイヤーまでの距離を測る
        float distance = Vector3.Distance(transform.position, target.position);

        // ★変更点：感知範囲に入っているかチェック
        if (distance <= detectionRange)
        {
            // --- 範囲内なら追いかける ---
            agent.isStopped = false;
            agent.SetDestination(target.position);

            // アニメーション：動く速度を入れる
            animator.SetFloat("Speed", agent.velocity.magnitude);
            
            // 攻撃範囲（近すぎたら止まる）
            if (distance <= stopDistance)
            {
                agent.isStopped = true;
                // ここで攻撃アニメーションなど
            }
        }
        else
        {
            // --- 範囲外なら待機 ---
            agent.isStopped = true;
            animator.SetFloat("Speed", 0f); // 止まるアニメーション
        }
    }
    // ★重要：矢から呼ばれるダメージ処理関数
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHP -= damage;
        Debug.Log($"敵がダメージを受けた！ 残りHP: {currentHP}");

        // 死亡判定
        if (currentHP <= 0)
        {
            Die();
        }
        else
        {
            // ダメージモーションがあればここで再生
            // animator.SetTrigger("Hit");
        }
    }

    private void Die()
    {
        isDead = true;
        agent.isStopped = true; // 移動停止
        agent.enabled = false;  // NavMeshAgentをオフ
        
        // コライダーを消して、死体に矢が当たらないようにする
        Collider col = GetComponent<Collider>();
        if (col) col.enabled = false;

        // 死亡アニメーション再生
        animator.SetTrigger("Die"); 
        
        // パタッと倒れる物理挙動にしたい場合はRagdollなどを使いますが、
        // まずは単純に「その場で消える」か「倒れるモーション」でOK
        Debug.Log("敵を倒した！");
        Destroy(gameObject, 3.0f); // 3秒後に消滅
    }
}