using UnityEngine;



[RequireComponent(typeof(Rigidbody))]

public class ArrowController : MonoBehaviour

{

    public enum ArrowState { Idle, Nocked, Flying, Stuck }



    [Header("設定")]

    [SerializeField] private float embedDepth = 0.1f;

    [SerializeField] private float autoDestroyTime = 5f;

    [SerializeField] private Vector3 modelRotationOffset = new Vector3(90, 0, 0);



    [Header("コライダー設定（必ずアサインしてください）")]

    [SerializeField] private Collider tipCollider;   // ★ 矢じり（刺さる判定用）

    [SerializeField] private Collider shaftCollider; // ★ 持ち手（持つ判定・弾かれる用）



    [Header("参照（自動取得）")]

    [SerializeField] private Rigidbody rb;



    public ArrowState CurrentState { get; private set; } = ArrowState.Idle;



    private void Awake()

    {

        if (!rb) rb = GetComponent<Rigidbody>();

        // 設定忘れ防止の警告

        if (tipCollider == null || shaftCollider == null)

        {

            Debug.LogError("ArrowController: TipCollider または ShaftCollider が設定されていません！インスペクターで割り当ててください。");

        }

    }



    private void FixedUpdate()

    {

        if (CurrentState == ArrowState.Flying && rb.velocity.sqrMagnitude > 0.01f)

        {

            Quaternion lookRotation = Quaternion.LookRotation(rb.velocity);

            transform.rotation = lookRotation * Quaternion.Euler(modelRotationOffset);

        }

    }



    public void Launch(Vector3 velocity)

    {

        if (CurrentState == ArrowState.Flying) return;



        transform.SetParent(null);

        SetPhysicsEnabled(true); // 発射時は物理オン

        rb.isKinematic = false;

        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        rb.velocity = velocity;



        CurrentState = ArrowState.Flying;

    }



    public void OnNock()

    {

        CurrentState = ArrowState.Nocked;

        SetPhysicsEnabled(false); // つがえている時は物理オフ（持ち手判定は残すなら要調整）

        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;

    }



    /// <summary>

    /// 物理挙動のON/OFF切り替え

    /// </summary>

    public void SetPhysicsEnabled(bool isEnabled)

    {

        rb.isKinematic = !isEnabled;

       

        // 状況に応じてコライダーのオンオフ制御

        if (tipCollider) tipCollider.enabled = isEnabled;

        if (shaftCollider) shaftCollider.enabled = isEnabled;

    }



    private void OnCollisionEnter(Collision collision)

    {

        if (CurrentState != ArrowState.Flying) return;
        // プレイヤーや他の矢への衝突判定を除外
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Arrow")) return;

        // ★重要：衝突情報の詳細を取得
        // GetContact(0)で最初の接触点を取得し、thisColliderで「自分のどのコライダーが当たったか」を確認
        ContactPoint contact = collision.GetContact(0);
        Collider myCollider = contact.thisCollider;

        // ★判定：矢じりのコライダーが当たった場合のみ「刺さる」処理をする

        if (myCollider == tipCollider)
        {
            AudioManager.Instance.PlaySE("hit", collision.contacts[0].point);
            // 当たった相手（またはその親）から SimpleEnemyAI スクリプトを探す
            var enemy = collision.gameObject.GetComponentInParent<SimpleEnemyAI>();

            // もし敵だったらダメージを与える
            if (enemy != null)
            {
                // ダメージ量はここで決める（例: 50ダメージ）
                enemy.TakeDamage(50f); 
                Debug.Log("敵に命中しました！");
            }
            StickArrow(collision, contact.point);

            ProcessScore(collision, contact.point);

        }

        else

        {

            // シャフト（持ち手）が当たった場合は、刺さらずにそのまま物理演算で弾かれる（何もしなくて良い）

            // 必要ならここに「カラン」という音を鳴らす処理などを追加

            Debug.Log("シャフトが当たりました（刺さりません）");

        }

    }



    private void ProcessScore(Collision collision, Vector3 hitPoint)

    {

        TargetBoard target = collision.gameObject.GetComponentInParent<TargetBoard>();

        if (target != null)

        {

            int score = target.CalculateScore(hitPoint);

            if (ScoreManager.instance != null)

            {

                ScoreManager.instance.AddScore(score);

            }

            Debug.Log($"Hit! Score: {score}");

        }

    }



    private void StickArrow(Collision collision, Vector3 hitPoint)

    {

        Vector3 stickDirection = transform.forward;

        if (stickDirection.sqrMagnitude < 0.001f) stickDirection = transform.forward;



        CurrentState = ArrowState.Stuck;



        // 物理停止

        rb.velocity = Vector3.zero;

        rb.angularVelocity = Vector3.zero;

        rb.isKinematic = true;

        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;



        // 刺さった後はコライダーを無効化（あるいはTrigger化）して、邪魔にならないようにする

        // ※ 持つためにシャフトのコライダーだけ残したい場合は shaftCollider.enabled = true にする

        tipCollider.enabled = false;

        shaftCollider.enabled = true; // 刺さった後も抜くために持つならON



        transform.position += stickDirection * embedDepth;

        transform.SetParent(collision.transform, true);



        if (autoDestroyTime > 0f)

        {

            Destroy(gameObject, autoDestroyTime);

        }

    }

}