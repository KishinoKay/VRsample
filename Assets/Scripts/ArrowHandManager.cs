using UnityEngine;
using UnityEngine.InputSystem;

public class ArrowHandManager : MonoBehaviour
{
    [Header("設定")]
    [Tooltip("矢を持つ位置")]
    public Transform arrowHolderPoint;
    [Tooltip("矢を拾える範囲(半径)")]
    public float pickupRadius = 0.15f;
    [Tooltip("矢を判定するためのレイヤー（指定なければAllでOK）")]
    public LayerMask arrowLayer = ~0; // デフォルトは全て

    [Header("入力設定")]
    public InputActionProperty toggleArrowAction;

    // 内部状態
    private GameObject currentHeldArrow;
    public bool HasArrow => currentHeldArrow != null;

    void OnEnable()
    {
        if (toggleArrowAction.action != null)
            toggleArrowAction.action.Enable(); 

        toggleArrowAction.action.performed += OnToggleArrow;
    }

    void OnDisable()
    {
        toggleArrowAction.action.performed -= OnToggleArrow;
        
        if (toggleArrowAction.action != null)
            toggleArrowAction.action.Disable();
    }

    // トリガーボタン処理
    private void OnToggleArrow(InputAction.CallbackContext context)
    {
        Debug.Log("ボタンが押されました！");
        if (HasArrow)
        {
            // 持っていれば落とす
            DropArrow();
        }
        else
        {
            // 持っていなければ近くの矢を探して拾う
            TryPickupArrow();
        }
    }

    // 周辺の矢を探して拾う処理
    private void TryPickupArrow()
    {
        // 手の周辺にあるコライダーを検出
        Collider[] hits = Physics.OverlapSphere(transform.position, pickupRadius, arrowLayer);
        Debug.Log($"周辺のコライダー検知数: {hits.Length}"); // ← 0なら範囲外かレイヤー違い

        foreach (var hit in hits)
        {
            Debug.Log($"検知したオブジェクト: {hit.name}, タグ: {hit.tag}"); // ← タグが "Arrow" か確認
            // "Arrow" タグがついている、かつ Rigidbody を持っているものを探す
            if (hit.CompareTag("Arrow"))
            {
                // 親（＝矢のルートオブジェクト）を取得
                // ※コライダーが子にある場合などを考慮して attachedRigidbody から探すのが確実
                Rigidbody targetRb = hit.attachedRigidbody;
                
                if (targetRb != null)
                {
                    EquipArrow(targetRb.gameObject);
                    break; // 1つ拾ったら終了
                }
            }
        }
    }

    // 指定された矢を装備する処理
    private void EquipArrow(GameObject arrowObj)
    {
        currentHeldArrow = arrowObj;

        // 物理演算無効化 & 親子関係設定
        Rigidbody rb = currentHeldArrow.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;

        // 手のホールド位置に移動
        currentHeldArrow.transform.SetParent(arrowHolderPoint);
        currentHeldArrow.transform.localPosition = Vector3.zero;
        currentHeldArrow.transform.localRotation = Quaternion.identity;

        // 持っている間はコライダーを無効化（弓や体に当たらないように）
        // 矢に複数のコライダーがある場合を考慮して配列で処理
        Collider[] cols = currentHeldArrow.GetComponentsInChildren<Collider>();
        foreach (var c in cols) c.enabled = false;
    }

    // 矢を落とす
    public void DropArrow()
    {
        if (!HasArrow) return;

        currentHeldArrow.transform.SetParent(null);

        Rigidbody rb = currentHeldArrow.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = false;

        Collider[] cols = currentHeldArrow.GetComponentsInChildren<Collider>();
        foreach (var c in cols) c.enabled = true;

        currentHeldArrow = null;
    }

    // 弓に矢を渡す用（BowControllerから呼ばれる）
    public GameObject GiveArrow()
    {
        if (!HasArrow) return null;

        GameObject arrowToGive = currentHeldArrow;
        currentHeldArrow = null; // 管理から外す
        
        return arrowToGive;
    }

    // デバッグ用：エディタ上で拾える範囲を表示
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}