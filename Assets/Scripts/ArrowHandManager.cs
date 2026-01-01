using UnityEngine;
using UnityEngine.InputSystem;

public class ArrowHandManager : MonoBehaviour
{
    [Header("設定")]
    [SerializeField] private Transform arrowHolderPoint;
    [SerializeField] private float pickupRadius = 0.15f;
    [SerializeField] private LayerMask arrowLayer = ~0;

    [Header("入力設定")]
    [SerializeField] private InputActionProperty grabAction;  // ★ 既存の矢を掴む・離す（例: Grip）
    [SerializeField] private InputActionProperty spawnAction; // ★ 新しい矢を生成する（例: Trigger / PrimaryButton）

    [Header("生成設定")]
    [SerializeField] private GameObject arrowPrefab;

    // 現在持っている矢
    private GameObject _currentHeldArrow;
    public bool HasArrow => _currentHeldArrow != null;

    private void OnEnable()
    {
        // --- Grab Action (掴む/離す) の登録 ---
        grabAction.action?.Enable();
        if (grabAction.action != null)
        {
            grabAction.action.performed += OnGrabInput;
        }

        // --- Spawn Action (生成) の登録 ---
        spawnAction.action?.Enable();
        if (spawnAction.action != null)
        {
            spawnAction.action.performed += OnSpawnInput;
        }
    }

    private void OnDisable()
    {
        if (grabAction.action != null) grabAction.action.performed -= OnGrabInput;
        grabAction.action?.Disable();

        if (spawnAction.action != null) spawnAction.action.performed -= OnSpawnInput;
        spawnAction.action?.Disable();
    }

    // ★ 掴む・離すボタンが押された時の処理
    private void OnGrabInput(InputAction.CallbackContext context)
    {
        if (HasArrow)
        {
            // 既に持っているなら -> 離す
            DropArrow();
        }
        else
        {
            // 持っていないなら -> 近くの矢を拾おうとする（生成はしない）
            TryPickupArrow();
        }
    }

    // ★ 生成ボタンが押された時の処理
    private void OnSpawnInput(InputAction.CallbackContext context)
    {
        // 既に矢を持っていたら何もしない（二重持ち防止）
        if (HasArrow) return;

        // 持っていなければ生成
        SpawnArrow();
    }

    public void SpawnArrow()
    {
        if (arrowPrefab == null)
        {
            Debug.LogWarning("ArrowHandManager: Arrow Prefabが設定されていません");
            return;
        }

        GameObject newArrow = Instantiate(arrowPrefab, arrowHolderPoint.position, arrowHolderPoint.rotation);
        EquipArrow(newArrow);
    }

    /// <summary>
    /// 近くの矢を探して装備する（生成機能は削除）
    /// </summary>
    private void TryPickupArrow()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, pickupRadius, arrowLayer);

        foreach (var hit in hits)
        {
            if (hit.attachedRigidbody != null && 
                hit.attachedRigidbody.TryGetComponent<ArrowController>(out var arrowCtrl))
            {
                if(arrowCtrl.CurrentState == ArrowController.ArrowState.Idle || 
                   arrowCtrl.CurrentState == ArrowController.ArrowState.Stuck)
                {
                    EquipArrow(arrowCtrl.gameObject);
                    return; // 1つ拾ったら終了
                }
            }
        }
    }

    private void EquipArrow(GameObject arrowObj)
    {
        _currentHeldArrow = arrowObj;
        
        if(_currentHeldArrow.TryGetComponent<ArrowController>(out var arrowCtrl))
        {
            arrowCtrl.SetPhysicsEnabled(false);
            // Stuck状態のものを拾った場合などのためにIdleに戻す
            // arrowCtrl.OnNock(); // 必要ならここでステート変更メソッドを呼ぶなど
        }

        _currentHeldArrow.transform.SetParent(arrowHolderPoint);
        _currentHeldArrow.transform.localPosition = Vector3.zero;
        _currentHeldArrow.transform.localRotation = Quaternion.identity;
    }

    public void DropArrow()
    {
        if (!HasArrow) return;

        _currentHeldArrow.transform.SetParent(null);

        if(_currentHeldArrow.TryGetComponent<ArrowController>(out var arrowCtrl))
        {
            arrowCtrl.SetPhysicsEnabled(true);
        }

        _currentHeldArrow = null;
    }

    public bool TryGetArrow(out GameObject arrow)
    {
        arrow = null;
        if (!HasArrow) return false;

        arrow = _currentHeldArrow;
        _currentHeldArrow = null;
        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}