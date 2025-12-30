using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class ArrowController : MonoBehaviour
{
    // 矢の状態定義
    public enum ArrowState { Idle, Nocked, Flying, Stuck }

    [Header("設定")]
    [SerializeField] private float embedDepth = 0.1f;
    [SerializeField] private float autoDestroyTime = 5f; // 0なら消えない
    [SerializeField] private Vector3 modelRotationOffset = new Vector3(90, 0, 0); // モデルの向き補正用

    [Header("参照（自動取得）")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Collider col;

    // 外部公開プロパティ
    public ArrowState CurrentState { get; private set; } = ArrowState.Idle;

    private void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody>();
        if (!col) col = GetComponent<Collider>();
    }

    private void FixedUpdate()
    {
        // 飛行中のみ、進行方向に向きを合わせる
        if (CurrentState == ArrowState.Flying && rb.velocity.sqrMagnitude > 0.01f)
        {
            // 速度方向への回転 + モデルごとのオフセット回転
            Quaternion lookRotation = Quaternion.LookRotation(rb.velocity);
            transform.rotation = lookRotation * Quaternion.Euler(modelRotationOffset);
        }
    }

    /// <summary>
    /// 矢を発射するメソッド
    /// </summary>
    public void Launch(Vector3 velocity)
    {
        if (CurrentState == ArrowState.Flying) return;

        transform.SetParent(null); // 親から切り離す（念のため）
        SetPhysicsEnabled(true);
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous; // すり抜け防止推奨
        rb.velocity = velocity;

        CurrentState = ArrowState.Flying;
    }

    /// <summary>
    /// 弓にセットされた時の処理
    /// </summary>
    public void OnNock()
    {
        CurrentState = ArrowState.Nocked;
        SetPhysicsEnabled(false);
        rb.collisionDetectionMode = CollisionDetectionMode.Discrete; // 負荷軽減
    }

    public void SetPhysicsEnabled(bool isEnabled)
    {
        rb.isKinematic = !isEnabled;
        col.enabled = isEnabled;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (CurrentState != ArrowState.Flying) return;

        // プレイヤーや他の矢への衝突判定を除外
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Arrow")) return;

        StickArrow(collision);
    }

    private void StickArrow(Collision collision)
    {
        // ★重要修正：停止させる前に、飛んできた方向（進行方向）を保存しておく
        // transform.forward は rotationOffset の影響で進行方向じゃない可能性があるため使わない
        Vector3 stickDirection = rb.velocity.normalized;

        // もし速度がほぼゼロ（ありえないが念のため）なら、衝突地点へのベクトルを使うなどの保険
        if (stickDirection.sqrMagnitude < 0.001f) stickDirection = transform.forward;

        CurrentState = ArrowState.Stuck;

        // 物理演算停止
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;

        // 親オブジェクト追従設定
        transform.SetParent(collision.transform);
        
        // ★修正：保存しておいた「進行方向」に向かってめり込ませる
        transform.position += stickDirection * embedDepth;

        // オプション：刺さったら少し揺らす演出などをここに入れると気持ちいいです

        if (autoDestroyTime > 0f)
        {
            Destroy(gameObject, autoDestroyTime);
        }
    }
}