using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))] // ★追加: 足音用のスピーカーを必須にする
public class SimpleEnemyAI : MonoBehaviour
{
    [Header("基本設定")]
    [SerializeField] private string targetTag = "Player";
    [SerializeField] private float maxHP = 100f;
    [SerializeField] private float stopDistance = 1.5f;
    [SerializeField] private float detectionRange = 10.0f;

    [Header("オーディオ設定")]
    [SerializeField] private string groanSE = "Zombie_Groan";
    [SerializeField] private float groanInterval = 5.0f;
    [SerializeField] private string footstepSE = "Footstep_Dirt";
    [SerializeField] private float footstepInterval = 0.5f;
    [SerializeField] private string deathSE = "Zombie_Death";

    private NavMeshAgent agent;
    private Animator animator;
    private Transform target;
    
    // ★追加: 足音専用のAudioSource
    private AudioSource footstepSource;

    private float currentHP;
    private bool isDead = false;
    private float groanTimer;
    private float footstepTimer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        
        // ★追加: AudioSourceコンポーネントを取得し、3D設定を行う
        footstepSource = GetComponent<AudioSource>();
        footstepSource.spatialBlend = 1.0f; // 完全3Dサウンドにする

        currentHP = maxHP;
        groanTimer = 0f;
        footstepTimer = 0f;


        // ★追加: ゲーム開始時に「私はここにいます」とマネージャーに登録
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterEnemy();
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag(targetTag);
        if (playerObj != null)
        {
            target = playerObj.transform;
        }
    }

    void Update()
    {
        if (isDead) return;

        // --- うめき声 (変更なし) ---
        groanTimer += Time.deltaTime;
        if (groanTimer >= groanInterval)
        {
            if (!string.IsNullOrEmpty(groanSE))
                AudioManager.Instance.PlaySE(groanSE, transform.position, 1.0f, 1.0f, 1.0f, 30.0f);
                Debug.Log("Enemy groans");
            groanTimer = 0f;
        }

        // --- ターゲット追尾 & 足音制御 ---
        if (target == null) return;

        float distance = Vector3.Distance(transform.position, target.position);

        // 追跡範囲内
        if (distance <= detectionRange)
        {
            // 攻撃範囲よりは遠い (移動中)
            if (distance > stopDistance)
            {
                agent.isStopped = false;
                agent.SetDestination(target.position);
                animator.SetFloat("Speed", agent.velocity.magnitude);

                // ★修正: 移動中の足音再生
                if (agent.velocity.magnitude > 0.1f)
                {
                    footstepTimer += Time.deltaTime;
                    if (footstepTimer >= footstepInterval)
                    {
                        if (!string.IsNullOrEmpty(footstepSE))
                        {
                            // ★変更: 自分のAudioSourceを使って再生 (これでStopできるようになる)
                            AudioManager.Instance.PlaySE(footstepSource, footstepSE);
                        }
                        footstepTimer = 0f;
                    }
                }
                else
                {
                    // 移動速度がほぼ0なら、念のため音を止める
                    // (壁に引っかかっている時などに足音が鳴り続けるのを防ぐ)
                    footstepSource.Stop();
                    footstepTimer = 0f; // 次動き出すときに即座に鳴らすなら0でOK
                }
            }
            // 攻撃範囲に到達 (待機/攻撃)
            else
            {
                StopMoving(); // ★処理を分離して呼び出し
            }
        }
        // 追跡範囲外 (待機)
        else
        {
            StopMoving(); // ★処理を分離して呼び出し
        }
    }

    // ★追加: 停止時の共通処理
    private void StopMoving()
    {
        agent.isStopped = true;
        animator.SetFloat("Speed", 0f);

        // ★ここで足音を強制停止！
        // 「ザッ…」という余韻も消えます
        if (footstepSource.isPlaying)
        {
            footstepSource.Stop();
        }
        
        // タイマーもリセットしておくと、次に動き出す時に変なタイミングで鳴らない
        footstepTimer = footstepInterval; 
    }

    // (以下 TakeDamage, Die などは変更なし)
    public void TakeDamage(float damage)
    {
        if (isDead) return;
        currentHP -= damage;
        if (currentHP <= 0) Die();
    }

    private void Die()
    {
        isDead = true;
        agent.isStopped = true;
        agent.enabled = false;
        
        // ★重要: 死んだ瞬間に足音は完全に止める
        footstepSource.Stop();

        Collider col = GetComponent<Collider>();
        if (col) col.enabled = false;

        animator.SetTrigger("Die");

        if (!string.IsNullOrEmpty(deathSE))
        {
            // 断末魔は「その場に残る音」として再生（死体と一緒に消えないように）
            AudioManager.Instance.PlaySE(deathSE, transform.position);
        }
        // ★追加: 死んだ時に「やられました」とマネージャーに報告
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReportEnemyDeath();
        }

        Destroy(gameObject, 3.0f);
    }
}