using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class BowController : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private Animator bowAnimator;
    [SerializeField] private Transform nockPoint;      // 弦の中心
    // 矢の先端側の支点
    [Tooltip("左手で弓を持った時に使う位置（右利きの人用）")]
    [SerializeField] private Transform restPointForLeftHandHolder; 
    
    [Tooltip("右手で弓を持った時に使う位置（左利きの人用）")]
    [SerializeField] private Transform restPointForRightHandHolder;
    [SerializeField] private Collider[] bowColliders;  // 自分自身のコライダー群

    [Header("パラメータ")]
    [SerializeField] private float maxPullDistance = 0.5f;
    [SerializeField] private float shotPower = 20f;
    [SerializeField] private string drawAnimStateName = "DrawBow";
    [SerializeField] private Vector3 arrowRotationOffset = new Vector3(90, 0, 0); // 矢のモデル補正
    [Header("アローレスト設定")]

    private float distanceoffset = 0.4f;//アローレストの位置調整用変数

    // 状態管理
    private Transform _holdingHand; // 弓を持っている手
    private Transform _pullingHand; // 弦を引いている手
    // 現在使用中のレストポイント
    private Transform _activeRestPoint;
    private ArrowController _currentArrow;
    private int _animStateHash;

    private bool _isPulled = false;

    private void Start()
    {
        _animStateHash = Animator.StringToHash(drawAnimStateName);
    }

    private void Update()
    {
        // 弦を引いている手が有効な場合のみ更新
        if (_pullingHand != null && _holdingHand != null)
        {
            UpdatePullingProcess();
        }
        else if (_isPulled)
        {
            // 引いていないのに引かれている状態が残っている場合のリセット（安全策）
            ResetBowString();
        }
    }

    private void UpdatePullingProcess()
    {
        _isPulled = true;

        // 【修正1】単純な距離ではなく、弓の向きを考慮した「引き代」を計算
        // ※ nockPoint.forward が「矢の飛ぶ方向」である前提です
        Vector3 pullDiff = _holdingHand.position - _pullingHand.position; // 持ち手 -> 引き手のベクトルだと逆になるので注意
        // 正しくは：引き手 - 持ち手 の位置関係を見る、あるいは nockPoint 基準で考える
        
        // より正確には「アローレスト」から「引き手」へのベクトルを、弓の発射軸に投影します
        Vector3 pullVector = _pullingHand.position - _activeRestPoint.position;
        
        // 弓の前方ベクトル（射線）
        Vector3 bowForward = ( _activeRestPoint.position - nockPoint.position ).normalized;
        // もし nockPoint.forward が信頼できるならそれでもOK
        // Vector3 bowForward = nockPoint.forward; 

        // 内積で「弓の後ろ方向」への成分を取り出す (-bowForward との内積)
        float pullValue = Vector3.Dot(pullVector, -bowForward);

        // 引き量がマイナス（弓より前に手がある）場合は0にする
        if(pullValue < 0) pullValue = 0;

        float pullProgress = Mathf.Clamp01((pullValue - distanceoffset) / maxPullDistance);

        // 2. アニメーション適用
        bowAnimator.Play(_animStateHash, 0, pullProgress);
        bowAnimator.speed = 0; 

        // 3. 矢の追従処理
        if (_currentArrow != null)
        {
            UpdateArrowTransform(pullProgress);
        }
    }

    private void UpdateArrowTransform(float pullProgress)
    {
        // 位置合わせ
        _currentArrow.transform.position = nockPoint.position;

        // 向き合わせ
        // 【修正】arrowRestPoint ではなく、左右判定済みの _activeRestPoint を使う
        Vector3 targetPos = (_activeRestPoint != null) ? _activeRestPoint.position : transform.position;
        
        Vector3 direction = targetPos - nockPoint.position;

        if (direction.sqrMagnitude > 0.001f)
        {
            _currentArrow.transform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(arrowRotationOffset);
        }
    }

    private void ResetBowString()
    {
        _isPulled = false;
        bowAnimator.Play(_animStateHash, 0, 0f);
    }

    // ---------------------------------------------------------
    // XR Interaction Toolkit Events
    // ---------------------------------------------------------

    // ■ 変更箇所：つかんだ時に「どっちの手か」を判定する
    public void OnBowGrabbed(SelectEnterEventArgs args)
    {
        _holdingHand = args.interactorObject.transform;
        
        // --- デバッグログ追加 ---
        //Debug.Log($"掴んだオブジェクト名: {_holdingHand.name}");
        //Debug.Log($"掴んだオブジェクトのタグ: {_holdingHand.tag}");
        // -----------------------

        // 手のタグで判定
        if (_holdingHand.CompareTag("LeftHand"))
        {
            //Debug.Log("判定: 左手 (LeftHand) -> 右利き用レストを使用");
            _activeRestPoint = restPointForLeftHandHolder;
        }
        else if (_holdingHand.CompareTag("RightHand"))
        {
            //Debug.Log("判定: 右手 (RightHand) -> 左利き用レストを使用");
            _activeRestPoint = restPointForRightHandHolder;
        }
        else
        {
            //Debug.LogWarning("判定不能: タグが一致しません。デフォルト(左手用)を使用します。");
            // タグ設定がない場合のフォールバック
            _activeRestPoint = restPointForLeftHandHolder;
        }
    }
    public void OnBowReleased(SelectExitEventArgs args) => _holdingHand = null;

    public void OnStringPulled(SelectEnterEventArgs args)
    {
        _pullingHand = args.interactorObject.transform;

        // 弦を引いた手に矢があるか確認
        // GetComponentInParent は負荷が高いので、可能ならInteractor自体にコンポーネントを持たせる設計が望ましい
        var handManager = args.interactorObject.transform.GetComponentInParent<ArrowHandManager>();
        
        if (handManager != null && handManager.TryGetArrow(out GameObject arrowObj))
        {
            if (arrowObj.TryGetComponent<ArrowController>(out var arrowCtrl))
            {
                NockArrow(arrowCtrl);
            }
        }
    }

    public void OnStringReleased(SelectExitEventArgs args)
    {
        FireArrow();
        _pullingHand = null;
        ResetBowString();
    }

    // ---------------------------------------------------------
    // 内部ロジック
    // ---------------------------------------------------------

    private void NockArrow(ArrowController arrow)
    {
        if (_currentArrow != null) return; // 既に装填済み

        _currentArrow = arrow;
        _currentArrow.transform.SetParent(nockPoint);
        _currentArrow.transform.localPosition = Vector3.zero;
        _currentArrow.transform.localRotation = Quaternion.identity;

        // 矢の状態を更新
        _currentArrow.OnNock();
    }

    private void FireArrow()
    {
        if (_currentArrow == null) return;

        _currentArrow.transform.SetParent(null);
        
        // 【修正2】見た目の向きと発射ベクトルを一致させる
        Vector3 targetPos = (_activeRestPoint != null) ? _activeRestPoint.position : transform.position;
        Vector3 aimDirection = (targetPos - nockPoint.position).normalized; // 弦 -> レスト の向き

        // 威力計算（UpdatePullingProcessで計算した進捗を使うのがベストですが、ここでも計算しなおすなら）
        // 単純距離ではなく、ここでもpullProgress的なロジックが理想ですが、
        // 簡易的には「nockPointとrestPointの距離」で代用可能です
        float currentDist = Vector3.Distance(targetPos, nockPoint.position);
        float powerMultiplier = Mathf.Clamp01((currentDist - distanceoffset) / maxPullDistance);

        Vector3 launchVelocity = aimDirection * (shotPower * powerMultiplier);

        IgnoreCollisionsForArrow(_currentArrow);
        _currentArrow.Launch(launchVelocity);
        _currentArrow = null;
    }

    private void IgnoreCollisionsForArrow(ArrowController arrow)
    {
        Collider[] arrowCols = arrow.GetComponentsInChildren<Collider>();
        
        // 弓本体との衝突無視
        foreach (var arrowCol in arrowCols)
        {
            foreach (var bowCol in bowColliders)
            {
                Physics.IgnoreCollision(arrowCol, bowCol, true);
            }
            
            // 手との衝突無視（もし必要なら）
            if (_holdingHand != null && _holdingHand.TryGetComponent<Collider>(out var handCol))
            {
                Physics.IgnoreCollision(arrowCol, handCol, true);
            }
        }
    }
}