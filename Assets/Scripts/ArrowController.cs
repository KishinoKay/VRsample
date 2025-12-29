using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ArrowController : MonoBehaviour
{
    [Header("設定")]
    public float embedDepth = 0.1f;
    public float autoDestroyTime = 0f;

    // 内部変数
    private Rigidbody rb;
    private bool isFlying = false; // デフォルトはfalse
    private Collider[] myColliders;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        myColliders = GetComponentsInChildren<Collider>();
    }

    void FixedUpdate()
    {
        // isFlyingがtrueの時だけ、矢の向きを変える
        if (isFlying && !rb.isKinematic && rb.velocity.sqrMagnitude > 0.1f)
        {
            transform.rotation = Quaternion.LookRotation(rb.velocity);
        }
    }

    // 【修正点】勝手に速度で判定していたUpdateメソッドを削除しました。
    /* void Update() { ... } 
    */

    // 【追加】弓から呼ばれる「発射通知」メソッド
    public void Launch()
    {
        isFlying = true;
    }

    void OnCollisionEnter(Collision collision)
    {
        // 飛んでいない（ただの落下や手放し）なら刺さらない
        if (rb.isKinematic || !isFlying) return;

        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Arrow")) return;

        StickArrow(collision);
    }

    private void StickArrow(Collision collision)
    {
        isFlying = false; // 刺さったら飛行終了

        rb.isKinematic = true;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        transform.SetParent(collision.transform);
        transform.position += transform.forward * embedDepth;

        foreach(var c in myColliders) c.enabled = true;

        if (autoDestroyTime > 0f)
        {
            Destroy(gameObject, autoDestroyTime);
        }
    }
}