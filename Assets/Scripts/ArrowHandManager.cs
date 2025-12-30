using UnityEngine;
using UnityEngine.InputSystem;

public class ArrowHandManager : MonoBehaviour
{
    [Header("設定")]
    [SerializeField] private Transform arrowHolderPoint;
    [SerializeField] private float pickupRadius = 0.15f;
    [SerializeField] private LayerMask arrowLayer = ~0; // デフォルトAll
    [SerializeField] private InputActionProperty toggleArrowAction;

    // 現在持っている矢
    private GameObject _currentHeldArrow;
    public bool HasArrow => _currentHeldArrow != null;

    private void OnEnable()
    {
        toggleArrowAction.action?.Enable();
        if (toggleArrowAction.action != null)
        {
            toggleArrowAction.action.performed += OnToggleInput;
        }
    }

    private void OnDisable()
    {
        if (toggleArrowAction.action != null)
        {
            toggleArrowAction.action.performed -= OnToggleInput;
        }
        toggleArrowAction.action?.Disable();
    }

    private void OnToggleInput(InputAction.CallbackContext context)
    {
        if (HasArrow)
        {
            DropArrow();
        }
        else
        {
            TryPickupArrow();
        }
    }

    private void TryPickupArrow()
    {
        // 範囲内のコライダーを取得 (NonAlloc版を使うとさらにメモリ効率が良いが、今回は簡易版)
        Collider[] hits = Physics.OverlapSphere(transform.position, pickupRadius, arrowLayer);

        foreach (var hit in hits)
        {
            // ArrowControllerを持っているか確認（タグ文字列比較より安全）
            // attachedRigidbodyを経由することで、コライダーが子にあっても親のスクリプトを取れる
            if (hit.attachedRigidbody != null && 
                hit.attachedRigidbody.TryGetComponent<ArrowController>(out var arrowCtrl))
            {
                // まだ誰にも持たれていない（Idle状態）矢のみ拾えるなどの条件を追加可能
                if(arrowCtrl.CurrentState == ArrowController.ArrowState.Idle || 
                   arrowCtrl.CurrentState == ArrowController.ArrowState.Stuck)
                {
                    EquipArrow(arrowCtrl.gameObject);
                    break;
                }
            }
        }
    }

    private void EquipArrow(GameObject arrowObj)
    {
        _currentHeldArrow = arrowObj;
        
        // 物理無効化
        if(_currentHeldArrow.TryGetComponent<ArrowController>(out var arrowCtrl))
        {
            arrowCtrl.SetPhysicsEnabled(false);
        }

        // 位置合わせ
        _currentHeldArrow.transform.SetParent(arrowHolderPoint);
        _currentHeldArrow.transform.localPosition = Vector3.zero;
        _currentHeldArrow.transform.localRotation = Quaternion.identity;
    }

    public void DropArrow()
    {
        if (!HasArrow) return;

        _currentHeldArrow.transform.SetParent(null);

        // 物理有効化
        if(_currentHeldArrow.TryGetComponent<ArrowController>(out var arrowCtrl))
        {
            arrowCtrl.SetPhysicsEnabled(true);
            // 手放した直後はIdle状態にする
            // arrowCtrl.Launch(Vector3.zero); // 必要ならここで軽く投げる処理も可能
        }

        _currentHeldArrow = null;
    }

    /// <summary>
    /// 弓に矢を渡すためのメソッド（成功したらtrueを返し、outで矢を渡す）
    /// </summary>
    public bool TryGetArrow(out GameObject arrow)
    {
        arrow = null;
        if (!HasArrow) return false;

        arrow = _currentHeldArrow;
        _currentHeldArrow = null; // 所有権を放棄
        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}